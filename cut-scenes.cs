using System;
using System.IO;
using System.Collections.Generic;


class Test
{
	public static int i32(FileStream stream)
	{				
		var chunk = new byte[4]; stream.Read(chunk, 0, 4);
		return BitConverter.ToInt32(chunk, 0);
	}

	public static bool FileOp(string file)
	{
		int n = 0, n0 = 0, a0 = 0; bool b = false;
		var chunk = new byte[4];
		using (var stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite))
		{
			if (i32(stream) != 0x53546F63) { Console.WriteLine(" not coTS"); return false; };
			stream.Seek(4, SeekOrigin.Current);
			stream.Read(chunk, 0, 4);
			if (chunk[0] == 2)
			{
				stream.Seek(0x20, SeekOrigin.Begin); stream.Read(chunk, 0, 4);
				stream.Seek(System.BitConverter.ToInt32(chunk, 0) + 0x800, SeekOrigin.Begin);
			}
			else if (chunk[0] == 1) stream.Seek(0x800, SeekOrigin.Begin);
			else { Console.WriteLine(" not 1 (2)"); return false; }
			while (true)
			{
				if (i32(stream) != 0x0716) break;
				int sz = i32(stream);
				if (i32(stream) != 0x1802FFFF) { Console.WriteLine(" not 0x1802FFFF"); return false; }
				long eof = stream.Position + sz;
				int hsz = i32(stream); long eoh = stream.Position + hsz;
				int nsz = i32(stream);
				var sbuf = new byte[nsz];
				stream.Read(sbuf, 0, nsz);

				stream.Seek(0x10, SeekOrigin.Current);
				int tsz = i32(stream);
				sbuf = new byte[tsz];
				stream.Read(sbuf, 0, tsz);

				var tp4 = BitConverter.ToInt32(sbuf, 0);

				//shot/seqm
				if (tp4 == 0x4E495254) //if (nsz==0x20) 					
				{
					stream.Seek(eoh, SeekOrigin.Begin);

					int a = 0; n0++;

					var j = (eof - stream.Position) / 4;
					for (long i = 0; i < j; i++)
					{
						if (i32(stream) == 0x00080101)
						{
							i++;
							stream.Read(chunk, 0, 4); var flt = BitConverter.ToSingle(chunk, 0);
							if (flt > 0.1 && flt < 50)
							{
								stream.Seek(-4, SeekOrigin.Current);
								stream.Write(BitConverter.GetBytes((float)0), 0, 4);
								a++;
							}
						}
					}
					//Console.Write(" 0x{0:X} X ",stream.Position); Console.Write(" {0} ",a);
					a0 += a;
					if (a > 0) n++;

				}
				//sound/SMB/exa
				if (tp4 == 0x424D53) //if (nsz==0x18) 
				{
					stream.Seek(eoh + 0x50, SeekOrigin.Begin);//Console.Write(" C[]: 0x{0:X4} ",stream.Position);
					stream.Read(chunk, 0, 4); stream.Seek(-4, SeekOrigin.Current);
					if (BitConverter.ToInt32(chunk, 0) == 0x5F636769)
					{
						const string s1 = "igc_act0_stub.exa";
						//const string s1 = "sx_fightclub_bell_start_01.exa";
						foreach (var c in s1)
						{
							stream.WriteByte((byte)c);
						}
						stream.WriteByte((byte)0);
						b = true;
					}
					else { Console.Write(" not igc_! "); Console.ReadKey(true); }
				}

				stream.Seek(eof, SeekOrigin.Begin);
			}
		}
		Console.Write(" {0} ", a0); Console.Write(b ? 'S' : '_');
		//Console.Write('\n');
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
				 && File.Exists(Path.Combine(rootpath, "godfather.exe"))
				 && Directory.Exists(Path.Combine(rootpath, "godfather_v4")));
	}

	public static void nats(string exePt)
	{
		var bakPt = exePt + ".bak";
		var fe = File.Exists(bakPt);
		if (!fe) File.Copy(exePt, bakPt);

		using (var stream = File.Open(exePt, FileMode.Open, FileAccess.ReadWrite))
		{
			stream.Seek(0x457D0-1, SeekOrigin.Begin);
			if (stream.ReadByte() != 0x75)
			{
				Console.WriteLine("exe not supported --");
				return;
			}
			var b = stream.ReadByte();
			/*
			switch (b) {
					case 0x17: {};
					case 0: {};
					default: {};
			}
			*/
			if (b == 0x17)
			{
				stream.Seek(-1, SeekOrigin.Current);
				stream.WriteByte(0);
				Console.WriteLine("exe patched +");
			}
			else if (b == 0x0)
			{
				if (!fe)
				{
					stream.Seek(-1, SeekOrigin.Current);
					stream.WriteByte(0x17);
				}
				else
				{
					stream.Close();
					File.Copy(bakPt, exePt, true);
				}
				Console.WriteLine("exe unpatched -");
			}
			
		}
	}
	
	public static List <string> findIgcStrs(string GF1DIR)
	{
		var dirs0 = Directory.GetDirectories(Path.Combine(GF1DIR, "godfather_v4"), "*_igcs", SearchOption.AllDirectories);
		var IgcStrs = new List<string> { };

		foreach (var dir0 in dirs0)
        {
			var dirs1 = Directory.GetDirectories(dir0);
			foreach (var dir1 in dirs1)
			{
				var p1 = Path.Combine(dir1, Path.GetFileName(dir1)) + ".str";
				//Console.WriteLine(p1); Console.ReadKey(true);
				if (File.Exists(p1)) IgcStrs.Add(p1);				
			}
        }	
		return IgcStrs;
	}
	
	public static void o123(string GF1DIR,char v2)
	{
		//bool f = false; if (v2=='2') {Console.WriteLine("Keep backup files? y/n"); f = Console.ReadKey(true).KeyChar=='y';Console.WriteLine("ok");}
		var files = findIgcStrs(GF1DIR); // Directory.GetFiles(Path.Combine(GF1DIR, "godfather_v4"), "igc_act0*.str", SearchOption.AllDirectories);
		byte i = 0;
		ExportFile(GF1DIR);
		foreach (string file in files)
		{
			var v1 = Path.GetFileName(file);
			//if (v1.Length == "igc_act04_s12_01.str".Length) { } else continue; // deprecated			
			Console.Write(v1+' ');
			if (v2 == '1')
			{
				FileOpWrap(file);
			}
			if (v2 == '2')
			{
				if (File.Exists(file + ".bak"))
				{
					File.Copy(file + ".bak", file, true);
					//if (!f) File.Delete(file + ".bak");
					Console.Write(" +b ");
				}
			}
			if (v2 == '3') if (File.Exists(file + ".cut")) { File.Copy(file + ".cut", file, true); Console.Write(" +c "); }
			i++;
			Console.Write("\n");
		}
		Console.WriteLine("The number of files is {0}.", i);
		Console.WriteLine("Don'.");
	}

	public static bool UI(ref string GF1DIR)
	{
		try
		{
			while (true)
			{
				if (isGFDir(GF1DIR)) { break; }
				else { Console.WriteLine("please input game folder (drag and drop)"); GF1DIR = Console.ReadLine(); continue; }
			}

			Console.WriteLine("please input option:"
							  + "\n 1 - backup and cut scenes."
							  + "\n 2 - restore backups (if available)."
							  + "\n 3 - quick cut (if you did 1 and 2)" +
							  "\n 4 - no alt tab suspend (exe patch)" + 
							  "\n D - produce distribution patch (with backup)" + 
							  "\n x - exit");
			char v2;
			while (true)
			{
				v2 = Console.ReadKey(true).KeyChar;
				if ("1234Dx".Contains(v2 + "")) { Console.WriteLine("ok, {0}.", v2); break; }
			}
			if (v2 == 'x') { Console.WriteLine("Bye."); Console.ReadKey(true); return false; }
			if (v2 == 'D') { Dist(GF1DIR); }
			if (v2 == '4')
			{
				nats(Path.Combine(GF1DIR, "godfather.exe"));
			}
			if ("123".Contains(v2 + "")) o123(GF1DIR, v2);
			Console.WriteLine("\nPress any key");
			Console.ReadKey(true);
			Console.Clear();
		}
		catch (Exception e)
		{
			Console.WriteLine("The process failed: {0}", e.ToString()); Console.ReadKey();
		}
		return true;
	}
	
	public static void Dist(string GF1DIR)
	{
		//var root = Path.Combine(GF1DIR, "godfather_v4");
		var distDir = Path.Combine(GF1DIR, "_user", "gf1_cut_scenes");		
		var b = true;
		if (!b)
		{
			Console.WriteLine("Apply patch immediately? y/*"); 
			b = Console.ReadKey(true).KeyChar == 'y';
		}
		ExportFile(Path.Combine(distDir, "patch\\"));
		if (b) ExportFile(GF1DIR);
		//foreach (var strIn in Directory.GetFiles(root, "igc_act*.str", SearchOption.AllDirectories))
		foreach (var strIn in findIgcStrs(GF1DIR))
		{
			var v1 = Path.GetFileName(strIn);
			//if (v1.Length == "igc_act04_s12_01.str".Length) { } else continue; // deprecated
			Console.Write(v1 + ' ');

			var strInBak = strIn + ".bak";
			bool bak = false;
			if (File.Exists(strInBak)) { bak = true; File.Copy(strInBak, strIn, true); }

			var bakPT = strIn.Replace(GF1DIR, Path.Combine(distDir, "orig\\"));
			if (bak) { File.Copy(strInBak, bakPT, true);}
			if (File.Exists(bakPT)) { 
				if (!bak) { File.Copy(bakPT, strIn, true); bak = true; } 			
			}
			else
			{
				var destPt = bakPT;
				Directory.CreateDirectory(Path.GetDirectoryName(destPt));
				File.Copy(strIn, destPt, true);
			}

			var midPt = Path.Combine(distDir, "mid", Path.GetFileName(strIn));
			{
				var destPt = midPt;
				Directory.CreateDirectory(Path.GetDirectoryName(destPt));
				File.Copy(strIn, destPt, true);
			}

			FileOp(midPt);

			var pchPt = strIn.Replace(GF1DIR, Path.Combine(distDir, "patch\\"));

			Directory.CreateDirectory(Path.GetDirectoryName(pchPt));
			File.Copy(midPt, pchPt, true);

			if (b)
			{
				File.Copy(pchPt, strIn, true);
			}

			Console.Write("\n");
		}
	}

	public static void ExportFile(string destDir)
	{
		var b64str = "U0NIbCAAAABQVAAABgFl/YABAoUBAYQDAKxEoAEI/wBTQ0NsDAAAAAEAAABTQ0RsFAAAAAEAAAAAAAAAAAAAAFNDRWwIAAAA";
		var b64data = Convert.FromBase64String(b64str);
		var relPt = "audiostreams\\igc_act0\\igc_act0_stub.exa";
		var destPt = Path.Combine(destDir, relPt);
		if (File.Exists(destPt)) { }
		else
		{
			Directory.CreateDirectory(Path.GetDirectoryName(destPt));
			using (var stream = File.Open(destPt, FileMode.CreateNew, FileAccess.Write))
			{
				stream.Write(b64data, 0, b64data.Length);
			}
		}
	}

	public static void Main(string[] args)
	{
		string GF1DIR = "", rp1 = @"c:\Games\GF1", rp2 = "";
		if (args.Length == 1) rp2 = args[0];
		foreach (var s in new string[] { rp1, ".", "..", @"..\..", rp2 })
		{
			if (isGFDir(s)) { GF1DIR = s; break; }
		}
		
		while (true)
        {
			if (!UI(ref GF1DIR)) return;
		}
		
	}
}