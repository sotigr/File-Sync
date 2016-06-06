 
using System.Collections.Generic; 
namespace File_sync
{
    public class FileGroups
    {
        public static FileGroups current = null;
        public Dictionary<string,List<string>> Groups;
    }
}
