package nsocry.bridge;

public class AdapterTest {
    public static void main(String[] args) throws Exception {
        Class<?> canvas = Class.forName("aY");
        Class<?>[] loaded = {canvas};
        check(Agent.inspect(new Class<?>[0])[0], "STARTING");
        check(Agent.inspect(loaded)[0], "STARTING");
        String[][] cases = {{"cI", "MENU"}, {"bJ", "ACCOUNT_SCREEN"}, {"cH", "CHARACTER_SELECT"}, {"ba", "GAME_SCREEN"}};
        for (String[] c : cases) {
            java.lang.reflect.Constructor<?> constructor = Class.forName(c[0]).getDeclaredConstructor();
            constructor.setAccessible(true);
            Agent.field(canvas, "a", "dr").set(null, constructor.newInstance());
            String[] state = Agent.inspect(loaded);
            check(state[0], c[1]);
            check(state[1], c[0]);
            if (c[0].equals("cH")) check(state[2], "ninja1\nninja2");
            else check(state[2], "");
        }
        java.lang.reflect.Field account = Agent.class.getDeclaredField("account"); account.setAccessible(true); account.set(null, "acc01");
        java.lang.reflect.Field password = Agent.class.getDeclaredField("password"); password.setAccessible(true); password.set(null, "secret");
        java.lang.reflect.Field character = Agent.class.getDeclaredField("character"); character.setAccessible(true); character.set(null, "ninja2");
        java.lang.reflect.Constructor<?> menuConstructor = Class.forName("cI").getDeclaredConstructor();
        menuConstructor.setAccessible(true);
        Object menu = menuConstructor.newInstance();
        Agent.field(canvas, "a", "dr").set(null, menu);
        Class<?>[] runtime = {canvas, Class.forName("GameMidlet"), Class.forName("javax.microedition.lcdui.Display"),
                Class.forName("javax.microedition.midlet.MIDlet")};
        Agent.automate(Agent.inspect(runtime), runtime);
        check((String)Agent.field(menu.getClass(), "p", "java.lang.String").get(null), "acc01");
        check((String)Agent.field(menu.getClass(), "q", "java.lang.String").get(null), "secret");
        if (Agent.field(menu.getClass(), "action", "int").getInt(menu) != 1003) throw new AssertionError("Login action");
        java.lang.reflect.Constructor<?> selectConstructor = Class.forName("cH").getDeclaredConstructor();
        selectConstructor.setAccessible(true);
        Object select = selectConstructor.newInstance();
        Agent.field(canvas, "a", "dr").set(null, select);
        Agent.automate(Agent.inspect(runtime), runtime);
        if (Agent.field(select.getClass(), "q", "int").getInt(select) != 2
                || Agent.field(select.getClass(), "action", "int").getInt(select) != 1000)
            throw new AssertionError("Character action");
        java.lang.reflect.Constructor<?> dialog = Class.forName("aq").getDeclaredConstructor();
        dialog.setAccessible(true);
        Agent.field(canvas, "a", "aq").set(null, dialog.newInstance());
        check(Agent.inspect(loaded)[0], "DIALOG");
        System.out.println("Adapter tests passed: fields, dialog, direct login and exact character selection.");
    }
    static void check(String actual, String expected) {
        if (!actual.equals(expected)) throw new AssertionError(actual + " != " + expected);
    }
}
