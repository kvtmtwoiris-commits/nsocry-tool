package nsocry.bridge;

import java.io.*;
import java.lang.instrument.Instrumentation;
import java.lang.reflect.*;
import java.net.*;
import java.nio.charset.StandardCharsets;
import java.util.Base64;

/** Original NSOCry Pro code. Version-pinned adapter; does not modify game bytecode. */
public final class Agent {
    private static volatile String account;
    private static volatile String password;
    private static volatile String character;
    private static volatile String automation = "IDLE";
    private static volatile boolean loginSent;
    private static volatile boolean accountFilled;
    private static volatile boolean characterSent;
    public static void premain(String ignored, final Instrumentation instrumentation) {
        final String token = System.getenv("NSOCRY_BRIDGE_TOKEN");
        final String port = System.getenv("NSOCRY_BRIDGE_PORT");
        if (token == null || port == null) return;
        Thread worker = new Thread(new Runnable() {
            public void run() {
                try (Socket socket = new Socket()) {
                    socket.connect(new InetSocketAddress("127.0.0.1", Integer.parseInt(port)), 5000);
                    socket.setSoTimeout(5000);
                    BufferedReader input = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
                    PrintWriter output = new PrintWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8), true);
                    output.println("HELLO\t1\t" + token);
                    if (!"OK".equals(input.readLine())) return;
                    boolean supported = "1".equals(System.getenv("NSOCRY_BRIDGE_SUPPORTED"));
                    while (!socket.isClosed()) {
                        // Only one bounded request at a time; tool controls polling frequency.
                        String command = readCommand(input);
                        if (command == null) return;
                        if (command.startsWith("LOGIN\t")) {
                            String[] values = command.split("\t", -1);
                            if (values.length != 4) return;
                            account = decode(values[1]);
                            password = decode(values[2]);
                            character = decode(values[3]);
                            if (account.length() == 0 || password.length() == 0
                                    || account.length() > 100 || password.length() > 100 || character.length() > 100) return;
                            automation = "READY";
                            output.println("ACK");
                            continue;
                        }
                        if (!"POLL".equals(command)) return;
                        String[] state;
                        try {
                            state = supported ? inspect(instrumentation.getAllLoadedClasses())
                                    : emptyState("UNSUPPORTED", "DISABLED");
                            if (supported && account != null) automate(state, instrumentation.getAllLoadedClasses());
                        } catch (Exception ex) {
                            // Never serialize exception messages or arbitrary game fields: they may contain secrets.
                            automation = "ERROR";
                            state = emptyState("ADAPTER_ERROR", automation);
                        } catch (LinkageError ex) {
                            automation = "ERROR";
                            state = emptyState("ADAPTER_ERROR", automation);
                        }
                        output.println("STATE\t" + state[0] + "\t" + encode(state[1]) + "\t" + encode(state[2])
                                + "\t" + encode(state[3]) + "\t" + state[4] + "\t" + encode(state[5]));
                        if (output.checkError()) return;
                    }
                } catch (Exception ex) {
                    // Losing the manager must not terminate the game or print credentials.
                }
            }
        }, "NSOCry-Bridge");
        worker.setDaemon(true);
        worker.start();
    }

    private static String readCommand(Reader input) throws IOException {
        StringBuilder s = new StringBuilder();
        for (int i = 0; i < 8192; i++) {
            int c = input.read();
            if (c == -1) return null;
            if (c == '\n') return s.toString();
            if (c != '\r') s.append((char)c);
        }
        throw new IOException("Oversize command");
    }

    static String[] inspect(Class<?>[] loaded) throws Exception {
        Class<?> canvas = null;
        // Do not Class.forName an uninitialized client: use only classes already loaded by its MIDlet loader.
        for (Class<?> c : loaded) if (c.getName().equals("aY")) {
            if (canvas != null && canvas != c) return emptyState("ADAPTER_ERROR", automation);
            canvas = c;
        }
        if (canvas == null) return emptyState("STARTING", automation);
        Object screen = field(canvas, "a", "dr").get(null);
        if (screen == null) return emptyState("STARTING", automation);
        String name = screen.getClass().getName();
        // aY.a:dr is the screen rendered by aY.a(Graphics), verified in the supplied client.
        String phase = name.equals("cI") ? "MENU" : name.equals("bJ") ? "ACCOUNT_SCREEN"
                : name.equals("cH") ? "CHARACTER_SELECT" : name.equals("ba") ? "GAME_SCREEN" : "OTHER_SCREEN";
        String characters = "";
        if (name.equals("cH")) {
            Object value = field(screen.getClass(), "F", "[Ljava.lang.String;").get(screen);
            if (value instanceof String[]) {
                StringBuilder names = new StringBuilder();
                for (String character : ((String[]) value).clone()) {
                    if (character == null || character.length() == 0) continue;
                    if (names.length() > 0) names.append('\n');
                    names.append(character.substring(0, Math.min(100, character.length())));
                    if (names.length() > 1000) break;
                }
                characters = names.toString();
            }
        }
        // Dialogs can cover a screen. Report that separately instead of claiming the underlying screen is ready.
        Object dialog = field(canvas, "a", "aq").get(null);
        if (dialog != null) phase = "DIALOG";
        String mapId = "-1";
        String mapName = "";
        if (phase.equals("GAME_SCREEN")) {
            Class<?> map = findOptional(loaded, "dg");
            if (map != null) {
                mapId = Short.toString(field(map, "X", "short").getShort(null));
                Object currentName = field(map, "hT", "java.lang.String").get(null);
                if (currentName instanceof String)
                    mapName = ((String) currentName).substring(0, Math.min(100, ((String) currentName).length()));
            }
        }
        // A transition during this sample invalidates the whole snapshot.
        if (field(canvas, "a", "dr").get(null) != screen) return emptyState("TRANSITION", automation);
        return new String[] {phase, name, characters, automation, mapId, mapName};
    }

    private static String[] emptyState(String phase, String automationState) {
        return new String[] {phase, "", "", automationState, "-1", ""};
    }

    static void automate(String[] state, Class<?>[] loaded) throws Exception {
        if ("DIALOG".equals(state[0]) || "TRANSITION".equals(state[0])) return;
        final Class<?> canvas = find(loaded, "aY");
        final Object screen = field(canvas, "a", "dr").get(null);
        if ("MENU".equals(state[0]) && !loginSent) {
            // cI.a(1003,Object) copies pending p/q to saved K/n, persists RMS and calls cK.c(login).
            field(screen.getClass(), "p", "java.lang.String").set(null, account);
            field(screen.getClass(), "q", "java.lang.String").set(null, password);
            loginSent = true;
            automation = "LOGIN_SENT";
            invokeOnGameThread(loaded, new Runnable() { public void run() {
                try { action(screen, 1003); } catch (Exception ex) { automation = "ERROR"; }
            }});
        } else if ("ACCOUNT_SCREEN".equals(state[0]) && !loginSent && !accountFilled) {
            // Some RMS states open bJ directly. Fill its two visible inputs, then action 2000 returns to menu.
            Object accountInput = field(screen.getClass(), "e", "db").get(screen);
            Object passwordInput = field(screen.getClass(), "f", "db").get(screen);
            method(accountInput.getClass(), "ah", String.class).invoke(accountInput, account);
            method(passwordInput.getClass(), "ah", String.class).invoke(passwordInput, password);
            accountFilled = true;
            automation = "ACCOUNT_FILLED";
            invokeOnGameThread(loaded, new Runnable() { public void run() {
                try { action(screen, 2000); } catch (Exception ex) { automation = "ERROR"; }
            }});
        } else if ("CHARACTER_SELECT".equals(state[0]) && !characterSent) {
            password = null; // Authentication succeeded far enough to receive the server character list.
            String[] names = (String[])field(screen.getClass(), "F", "[Ljava.lang.String;").get(screen);
            int selected = -1;
            for (int i = 0; i < names.length; i++) {
                if (names[i] != null && (character.length() == 0 || names[i].equalsIgnoreCase(character))) {
                    selected = i; break;
                }
            }
            if (selected < 0) { automation = "CHARACTER_NOT_FOUND"; return; }
            field(screen.getClass(), "q", "int").setInt(screen, selected);
            characterSent = true;
            automation = "CHARACTER_SENT";
            invokeOnGameThread(loaded, new Runnable() { public void run() {
                try { action(screen, 1000); } catch (Exception ex) { automation = "ERROR"; }
            }});
        } else if ("GAME_SCREEN".equals(state[0])) {
            password = null;
            automation = "COMPLETE";
        }
    }

    private static void action(Object screen, int id) throws Exception {
        method(screen.getClass(), "a", Integer.TYPE, Object.class).invoke(screen, Integer.valueOf(id), null);
    }

    private static void invokeOnGameThread(Class<?>[] loaded, Runnable action) throws Exception {
        Class<?> midletClass = find(loaded, "GameMidlet");
        Object midlet = field(midletClass, "a", "GameMidlet").get(null);
        if (midlet == null) throw new IllegalStateException();
        Class<?> displayClass = find(loaded, "javax.microedition.lcdui.Display");
        Class<?> midletBase = find(loaded, "javax.microedition.midlet.MIDlet");
        Object display = displayClass.getMethod("getDisplay", midletBase).invoke(null, midlet);
        displayClass.getMethod("callSerially", Runnable.class).invoke(display, action);
    }

    private static Class<?> find(Class<?>[] loaded, String name) throws ClassNotFoundException {
        Class<?> found = null;
        for (Class<?> c : loaded) if (c.getName().equals(name)) {
            if (found != null && found != c) throw new ClassNotFoundException(name);
            found = c;
        }
        if (found == null) throw new ClassNotFoundException(name);
        return found;
    }

    private static Class<?> findOptional(Class<?>[] loaded, String name) throws ClassNotFoundException {
        Class<?> found = null;
        for (Class<?> c : loaded) if (c.getName().equals(name)) {
            if (found != null && found != c) throw new ClassNotFoundException(name);
            found = c;
        }
        return found;
    }

    private static Method method(Class<?> owner, String name, Class<?>... types) throws NoSuchMethodException {
        Method value = owner.getDeclaredMethod(name, types);
        value.setAccessible(true);
        return value;
    }

    static Field field(Class<?> owner, String name, String type) throws NoSuchFieldException {
        // Obfuscated bytecode contains fields sharing a name but with DIFFERENT descriptors.
        for (Field f : owner.getDeclaredFields()) {
            if (f.getName().equals(name) && f.getType().getName().equals(type)) {
                f.setAccessible(true);
                return f;
            }
        }
        throw new NoSuchFieldException(name);
    }

    private static String encode(String value) {
        return Base64.getEncoder().encodeToString(value.getBytes(StandardCharsets.UTF_8));
    }

    private static String decode(String value) {
        return new String(Base64.getDecoder().decode(value), StandardCharsets.UTF_8);
    }
}
