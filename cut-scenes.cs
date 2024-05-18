using System;
using System.IO;

class Test
{
	public static int i32(FileStream stream)
	{
		var chunk = new byte[4];stream.Read(chunk, 0, 4);
		return BitConverter.ToInt32(chunk,0);
	}
	
	public static bool FileOp(string file)
	{
		int n = 0, n0 = 0, a0 = 0;bool b = false;
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
				int tmp;
				stream.Read(chunk, 0, 4); tmp = BitConverter.ToInt32(chunk,0);
				if (tmp != 0x0716) break;
				stream.Read(chunk, 0, 4); int sz = BitConverter.ToInt32(chunk,0);
				stream.Read(chunk, 0, 4); 
				if (BitConverter.ToInt32(chunk,0)!=0x1802FFFF) {Console.WriteLine(" not 0x1802FFFF"); return false;}
				long eof = stream.Position + sz; 
				stream.Read(chunk, 0, 4); int hsz = BitConverter.ToInt32(chunk,0); long eoh = stream.Position + hsz;
				stream.Read(chunk, 0, 4); int nsz = BitConverter.ToInt32(chunk,0);
				var sbuf = new byte[nsz];
				stream.Read(sbuf, 0, nsz);
				//var name = BitConverter.ToString(sbuf,0,nsz); Console.Write("{0}\t",name.Substring(17));
				//Console.Write(" name { ");
				/*
				foreach (var b in sbuf) {
					Console.Write((char)b);
				} //Console.Write(" } ");
				*/
				stream.Seek(0x10,SeekOrigin.Current);
				int tsz = i32(stream);
				sbuf = new byte[tsz];
				stream.Read(sbuf, 0, tsz);
				//var type = BitConverter.ToString(sbuf,0,tsz);
				//Console.Write(" type { ");
				/*
				foreach (var b in sbuf) {
					Console.Write((char)b); //Console.ReadKey(true);
				} //Console.Write(" } ");
				*/
				var tp4 = BitConverter.ToInt32(sbuf,0);
				//stream.Seek(nsz,SeekOrigin.Current);
				stream.Seek(eoh,SeekOrigin.Begin);
				//shot/xml/seq
				if (tp4==0x4E495254) //if (nsz==0x20) 					
				{
					//Console.Write(" k ");
					stream.Seek(0x10,SeekOrigin.Current);
					stream.Seek(i32(stream)-0x14,SeekOrigin.Current);
					//Console.Write(" 0x{0:X}-0x{1:X} ",stream.Position, eof); Console.Write(" {0} ", (eof-stream.Position)/4);
					int a = 0; var j = (eof-stream.Position)/4;n0++;
					for (long i = 0; i < j; i++) {
						stream.Read(chunk, 0, 4); var flt = BitConverter.ToSingle(chunk, 0);
						if (flt > 0.1 && flt < 50) {
							stream.Seek(-4, SeekOrigin.Current);
							stream.Write(BitConverter.GetBytes((float)-1.0), 0, 4); 
							a++;
						}
					}
					//Console.Write(" 0x{0:X} X ",stream.Position); Console.Write(" {0} ",a);
					a0 += a;
					if (a>0) n++;
					
				}					
				//sound/SMB/exa
				if (tp4==0x424D53) //if (nsz==0x18) 
				{
					stream.Seek(0x50,SeekOrigin.Current);//Console.Write(" C[]: 0x{0:X4} ",stream.Position);
					stream.Read(chunk, 0, 4);stream.Seek(-4,SeekOrigin.Current);
					if (BitConverter.ToInt32(chunk,0)==0x5F636769) {//Console.Write(" + ");
						const string s1 = "sx_fightclub_bell_start_01.exa";
						foreach ( var c in s1) {
							stream.WriteByte((byte)c);
						}
						//Console.Write(" S ");
						b = true;
					} else {Console.Write(" not igc_! "); Console.ReadKey(true);}
				}
				
				stream.Seek(eof,SeekOrigin.Begin);
				//Console.ReadKey(true);
			}
		}
		Console.Write(" {0} ",a0);Console.Write(b ? 'S' : '_'  );
		//Console.Write(" {0}/{1} ",n,n0);
		
		//Console.ReadKey(true);
		return true;
	}
	
	public static void FileOpWrap(string file)
	{
		if (File.Exists(file + ".bak")) File.Copy(file + ".bak", file, true);
		else File.Copy(file, file + ".bak", false);		
		FileOp(file);
		File.Copy(file, file + ".cut", true);
	}
	
	public static bool isGFDir(string rootpath)
	{
		return (Directory.Exists(rootpath)
			     && File.Exists(Path.Combine(rootpath,"godfather.exe")) 
			     && Directory.Exists(Path.Combine(rootpath,"godfather_v4")));
	}
	
	public static void nats(string exepath)
	{
		using (var stream = File.Open(exepath, FileMode.Open, FileAccess.ReadWrite)) 
		{
			stream.Seek(0x457D0,SeekOrigin.Begin);
			var b = stream.ReadByte();
			/*
			switch (b) {
					case 0x17: {};
					case 0: {};
					default: {};
			}
			*/
			if (b==0x17) 
			{
				stream.Seek(-1,SeekOrigin.Current);
				stream.WriteByte(0);
				Console.WriteLine("exe patched +");
			}
			else if (b==0x0) 
			{
				stream.Seek(-1,SeekOrigin.Current);
				stream.WriteByte(0x17);
				Console.WriteLine("exe unpatched -");
			}
			else 
			{
				Console.WriteLine("exe not supported --");
			}
		}
	}
	
    public static void Main0(string rootpath, bool t)
    {
    	Console.Clear();
        try
        {
        	if (!t)
        	while(true)
        	{
        		if (isGFDir(rootpath)) {break;}
			else {Console.WriteLine("please input game folder (drag and drop)"); rootpath = Console.ReadLine(); continue;}			
        	}
        	Directory.SetCurrentDirectory(rootpath);
        	Console.WriteLine("please input option:"
        	                  +"\n 1 - backup and cut scenes."
        	                  +"\n 2 - restore backups (if available)."
        	                  +"\n 3 - quick cut (if you did 1 and 2)."+
        	                  "\n 4 - no alt tab suspend (exe patch)."
        	                  +"\n x - exit");
        	char v2;
        	while(true) {
        	v2 = Console.ReadKey(true).KeyChar;
        	if ("1234x".Contains(v2+"")) {Console.WriteLine("ok, {0}.",v2);break;}
        	}
        	//bool f = false; if (v2=='2') {Console.WriteLine("Keep backup files? y/n"); f = Console.ReadKey(true).KeyChar=='y';Console.WriteLine("ok");}
        	if (v2=='x') {Console.WriteLine("Bye.");Console.ReadKey();return;}
        	if (v2 == '4') { 
        		nats(Path.Combine(rootpath,"godfather.exe")); 
        		Console.ReadKey();
        		Main0(rootpath,t);
        	}
            string[] files = Directory.GetFiles(".", "igc_act*.str",SearchOption.AllDirectories);
            byte i =0;
            foreach (string file in files)
            {
            	var v1=Path.GetFileName(file);
            	if (v1.Length=="igc_act04_s12_01.str".Length) {} else continue;
                Console.Write("{0} ",v1);
                if (v2 == '1') 
                {	
                	FileOpWrap(file);
                }
                if (v2 == '2') 
                {                	
                	if (File.Exists(file + ".bak")) {File.Copy(file+ ".bak", file, true);
                		//if (!f) File.Delete(file + ".bak");
                		Console.Write(" +b ");
                	}
                }
                if (v2 == '3') if (File.Exists(file + ".cut")) {File.Copy(file+ ".cut", file, true);Console.Write(" +c ");}
                i++;
                Console.Write("\n");
            }
            Console.WriteLine("The number of files is {0}.", i);
            Console.WriteLine("Don'.");
            Console.ReadKey(true);
            Console.Clear();
            Main0(rootpath,t);
        }
        catch (Exception e)
        {
            Console.WriteLine("The process failed: {0}", e.ToString());Console.ReadKey();
        }
    }
    
    
    public static void Main(string[] args)
    {
    	//FileOpWrap(@"C:\Games\GF1\godfather_v4\godfather\missions\training\t2_igcs\igc_act01_t02_01\igc_act01_t02_01.str");return;
    	string rp="",rp1 =@"c:\Games\GF1",rp2=""; bool t = false;
    	if (args.Length == 1) rp2 = args[0];
    	foreach (var s in new string[] {rp1,".","..",@"..\..",rp2}) {
    		if (isGFDir(s)) {rp=s;t = true;break;}
    	}
    	Main0(rp,t);
    }
}