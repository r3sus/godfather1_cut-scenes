using System;
using System.IO;

class Test
{
	
	public static void FileOp0(string file)
	{
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
				Console.Write("{0} \n",myFloat);
				if (myFloat>0.1) stream.Write(System.BitConverter.GetBytes((float)1), 0, 4);
				stream.Seek(pos,SeekOrigin.Begin);
			}			
			}
			
		}
		
		Console.WriteLine("wrote {0} values",i);
		//Console.ReadKey(true);
	}
	
	public static bool FileOp(string file)
	{
		byte a=0,b = 0;
		var chunk = new byte[4];
		using (var stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite)) {
			stream.Read(chunk, 0, 4);
			if (System.BitConverter.ToInt32(chunk,0) != 0x53546F63) {Console.WriteLine(" not coTS"); return false;};
			stream.Seek(4,SeekOrigin.Current);
			stream.Read(chunk, 0, 4);			
			if (chunk[0] == 2) {stream.Seek(0x20,SeekOrigin.Begin);stream.Read(chunk, 0, 4);
				stream.Seek(System.BitConverter.ToInt32(chunk,0)+0x800,SeekOrigin.Begin);}
			else if (chunk[0] == 1) stream.Seek(0x800,SeekOrigin.Begin);			
			else {Console.WriteLine(" not 1 (2)"); return false;}
			while (true)
			{
				int sz;
				stream.Read(chunk, 0, 4); sz = BitConverter.ToInt32(chunk,0);
				if (sz != 0x0716) break;
				stream.Read(chunk, 0, 4); sz = BitConverter.ToInt32(chunk,0);
				stream.Read(chunk, 0, 4); 
				if (BitConverter.ToInt32(chunk,0)!=0x1802FFFF) {Console.WriteLine(" not 0x1802FFFF"); return false;}
				var pos = stream.Position + sz;
				stream.Read(chunk, 0, 4); var hz = BitConverter.ToInt32(chunk,0)+stream.Position;
				stream.Read(chunk, 0, 4); sz = BitConverter.ToInt32(chunk,0);
				//var b1 = new byte[sz]; stream.Read(b1, 0, sz);
				stream.Seek(sz,SeekOrigin.Current);
				stream.Seek(0x14,SeekOrigin.Current);
				stream.Read(chunk, 0, 4); sz = BitConverter.ToInt32(chunk,0);
				//if (sz==0xCC || sz==0xC4) 
				if (sz==0x4E495254)
				{
					stream.Seek(pos-8,SeekOrigin.Begin);//Console.Write(" FLT: 0x{0:X8} ",stream.Position);
					stream.Read(chunk, 0, 4);
					if (BitConverter.ToSingle(chunk, 0) > 0.1) {//Console.Write(" + ");
						a++;
						stream.Seek(-4,SeekOrigin.Current);
						stream.Write(BitConverter.GetBytes((float)1.0), 0, 4);
					}					
				}
				if (sz==0x424D53)
				{
					stream.Seek(hz+0x50,SeekOrigin.Begin);//Console.Write(" C[]: 0x{0:X4} ",stream.Position);
					stream.Read(chunk, 0, 4);stream.Seek(-4,SeekOrigin.Current);
					if (BitConverter.ToInt32(chunk,0)==0x5F636769) {//Console.Write(" + ");
						b++;
						const string s1 = "sx_fightclub_bell_start_01.exa";
						foreach ( var c in s1) {
							stream.WriteByte((byte)c);
						}
						//stream.Write(BitConverter.GetBytes(s1,
					} else {Console.Write(" not igc_! "); Console.ReadKey(true);}
				}
				
				stream.Seek(pos,SeekOrigin.Begin);
				//{Console.WriteLine(" not 0xCC (0xC4)"); return false;}
			}
		}
		
		Console.WriteLine("wrote {0} floats and {1} strings",a,b);
		//Console.ReadKey(true);
		//Console.Write('\n');
		return true;
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
                if (v2 == '1') 
                {
					if (File.Exists(file + ".bak")) File.Copy(file + ".bak", file, true);
					else File.Copy(file, file + ".bak", false);		
				FileOp(file);
					File.Copy(file, file + ".cut", true);	
                }
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
            Console.WriteLine("The process failed: {0}", e.ToString());Console.ReadKey();
        }
    }
    
    
    public static void Main(string[] args)
    {
    	string rp="",rp1 =@"c:\Games\GF1",rp2="";
    	if (args.Length == 1) rp2 = args[0];
    	foreach (var s in new string[] {rp1,".","..",@"..\..",rp2}) {
    		if (isGFDir(s)) {rp=s;break;}
    	}
    	Main0(rp);
    }
}