namespace Lechuza.UI
{
    public interface IWindowHost
    {
        void PushWindow(WindowBase window);
        void PopWindow();
        void PopScene();
    }
}
