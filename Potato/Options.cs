using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POTATO
{



    public class Options

    {
        public string PathX64 { get; set; }
        public string PathX86 { get; set; }
        public bool x86AppOnX64Os { get; set; }
        public string Version { get; set; }
        public bool Update { get; set; }
        public bool RemoveOtherVersion { get; set; }
        public bool ForceRestart { get; set; }
        public string Property { get; set; }
        public string UninstallPropertyEXE { get; set; }
        public bool NeedEXELog { get; set; }
        //public string PathToDirLog { get; set; }


        //  <?xml version = "1.0" encoding="utf-8"?>
        //  <Options xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
        //      <PathX64>\\fileserv.omsu.vmr\soft_distribution$\7-zip\1900\7z-x64.msi</PathX64>
        //      <PathX86>\\fileserv.omsu.vmr\soft_distribution$\7-zip\1900\7z.msi</PathX86>
        //      <Version></Version>
        //      <Update>true</Update>
        //      <x86AppOnX64Os>false</x86AppOnX64Os>
        //      <ForceRestart>false</ForceRestart>
        //      <Property />
        //  </Options>

        public Options() { }

    }
}
