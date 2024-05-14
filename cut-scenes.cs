using System;
using System.IO;

class Test
{
	
	public static void FileOp(string file)
	{
		if (File.Exists(file + ".bak")) File.Copy(file+ ".bak", file, true);
		else File.Copy(file, file + ".bak", false);		
		int i = 0;
		using (var stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite)) {
			for (int j = 0; j<stream.Length/4;j++)
			{
			var chunk = new byte[4];
			stream.Read(chunk, 0, 4);
			var pos = stream.Position;
			//float myFloat = System.BitConverter.ToSingle(chunk, 0);
			int myInt = System.BitConverter.ToInt32(chunk,0);
			//string myStr = System.BitConverter.ToString(chunk, 0);
			//Console.WriteLine(myStr);
			//Console.ReadKey(true);
			if (myInt==0x4E495254)
			//if (myFloat >= 0.1 && myFloat <= 50) 
			{
				//Console.Write("0x{0:X8}: ",pos);
				i++;
				stream.Seek(-84, SeekOrigin.Current);
				stream.Read(chunk, 0, 4);float myFloat = System.BitConverter.ToSingle(chunk, 0);
				stream.Seek(-4, SeekOrigin.Current);
				Console.Write("{0} ",myFloat);
				if (myFloat>0.1) stream.Write(System.BitConverter.GetBytes((float)1), 0, 4);
				stream.Seek(pos,SeekOrigin.Begin);
			}			
			}
			
		}
		File.Copy(file, file + ".cut", true);	Console.Write('\n');
		Console.WriteLine("wrote {0} values",i);
		//Console.ReadKey(true);
	}
	
	public static bool isGFDir(string rootpath)
	{
		return (Directory.Exists(rootpath)
			     && File.Exists(Path.Combine(rootpath,"godfather.exe")) 
			     && Directory.Exists(Path.Combine(rootpath,"godfather_v4")));
	}
    public static void Main0(string rootpath)
    {
        try
        {
        	while(true)
        	{
        		if (isGFDir(rootpath)) {break;}
			else {Console.WriteLine("please input game folder (drag and drop)"); rootpath = Console.ReadLine(); continue;}			
        	}
        	Directory.SetCurrentDirectory(rootpath);
        	Console.WriteLine("please input option:\n 1 - backup and cut scenes.\n 2 - restore (if you did 1).\n 3 - quick cut (if you did 1 and 2).\n x - exit");
        	char v2;
        	while(true) {
        	v2 = Console.ReadKey(true).KeyChar;
        	if ("123x".Contains(v2+"")) {Console.WriteLine("ok, {0}.",v2);break;}
        	}
        	if (v2=='x') {Console.WriteLine("Bye.");Console.ReadKey();return;}
            string[] files = Directory.GetFiles(".", "igc_act*.str",SearchOption.AllDirectories);
            byte i =0;
            foreach (string file in files)
            {
            	var v1=Path.GetFileName(file);
            	if (v1.Length=="igc_act04_s12_01.str".Length) {} else continue;
                Console.WriteLine(v1);
                if (v2 == '1') Test.FileOp(file);
                if (v2 == '2') if (File.Exists(file + ".bak")) File.Copy(file+ ".bak", file, true);
                if (v2 == '3') if (File.Exists(file + ".cut")) File.Copy(file+ ".cut", file, true);
                i++;
            }
            Console.WriteLine("The number of files is {0}.", i);
            Console.WriteLine("Don'.");
            Console.ReadKey();
            Console.Clear();
            Main0(rootpath);
        }
        catch (Exception e)
        {
            Console.WriteLine("The process failed: {0}", e.ToString());
        }
    }
    
    
    public static void Main(string[] args)
    {
    	string rp,rp1 =@"c:\Games\GF1",rp2="";
    	if (args.Length == 1) rp2 = args[0];
    	foreach (var s in new string[] {rp1,".","..",@"..\..",rp2}) {
    		if (isGFDir(s)) {rp=s;break;}
    	}
    	Main0(rp);
    }
}