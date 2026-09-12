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
        java.lang.reflect.Constructor<?> dialog = Class.forName("aq").getDeclaredConstructor();
        dialog.setAccessible(true);
        Agent.field(canvas, "a", "aq").set(null, dialog.newInstance());
        check(Agent.inspect(loaded)[0], "DIALOG");
        System.out.println("Adapter tests passed, including duplicate field names and dialog overlay.");
    }
    static void check(String actual, String expected) {
        if (!actual.equals(expected)) throw new AssertionError(actual + " != " + expected);
    }
}
