package javax.microedition.lcdui;
import javax.microedition.midlet.MIDlet;
public class Display {
    private static final Display VALUE = new Display();
    public static Display getDisplay(MIDlet ignored) { return VALUE; }
    public void callSerially(Runnable action) { action.run(); }
}
