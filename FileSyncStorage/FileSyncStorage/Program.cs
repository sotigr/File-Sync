using System.Windows.Forms; 
namespace FileSyncServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); 
            Application.Run(new MyApplicationContext());
        }
    }
}

