package nsocry.bridge;

import java.io.*;
import java.lang.instrument.Instrumentation;
import java.lang.reflect.*;
import java.net.*;
import java.nio.charset.StandardCharsets;
import java.util.Base64;

/** Original NSOCry Pro code. Read-only adapter; does not modify game bytecode. */
public final class Agent {
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
                        if (!"POLL".equals(command)) return;
                        String[] state;
                        try {
                            state = supported ? inspect(instrumentation.getAllLoadedClasses())
                                    : new String[] {"UNSUPPORTED", "", ""};
                        } catch (Exception ex) {
                            // Never serialize exception messages or arbitrary game fields: they may contain secrets.
                            state = new String[] {"ADAPTER_ERROR", "", ""};
                        } catch (LinkageError ex) {
                            state = new String[] {"ADAPTER_ERROR", "", ""};
                        }
                        output.println("STATE\t" + state[0] + "\t" + encode(state[1]) + "\t" + encode(state[2]));
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
        for (int i = 0; i < 32; i++) {
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
            if (canvas != null && canvas != c) return new String[] {"ADAPTER_ERROR", "", ""};
            canvas = c;
        }
        if (canvas == null) return new String[] {"STARTING", "", ""};
        Object screen = field(canvas, "a", "dr").get(null);
        if (screen == null) return new String[] {"STARTING", "", ""};
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
        // A transition during this sample invalidates the whole snapshot.
        if (field(canvas, "a", "dr").get(null) != screen) return new String[] {"TRANSITION", "", ""};
        return new String[] {phase, name, characters};
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
}
