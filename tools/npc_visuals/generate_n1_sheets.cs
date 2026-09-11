// Deterministic N-1 art authoring helper. Produces native pixel PNGs without external tooling.
// Sources remain editable in art/town-keyart-rework/npc/*.json (palette, silhouette, frame offsets).
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

static class N1Sheets
{
    const int Cell=150, W=750, H=750;
    static readonly uint[] CrcTable=MakeCrc();
    static uint[] MakeCrc(){var t=new uint[256]; for(uint n=0;n<256;n++){uint c=n;for(int k=0;k<8;k++)c=(c&1)!=0?0xedb88320^(c>>1):c>>1;t[n]=c;}return t;}
    static uint Crc(byte[] b){uint c=0xffffffff;foreach(byte x in b)c=CrcTable[(c^x)&255]^(c>>8);return c^0xffffffff;}
    static void Chunk(Stream s,string type,byte[] data){var tb=Encoding.ASCII.GetBytes(type);WriteU(s,(uint)data.Length);s.Write(tb);s.Write(data);var all=new byte[tb.Length+data.Length];Buffer.BlockCopy(tb,0,all,0,tb.Length);Buffer.BlockCopy(data,0,all,tb.Length,data.Length);WriteU(s,Crc(all));}
    static void WriteU(Stream s,uint v){s.WriteByte((byte)(v>>24));s.WriteByte((byte)(v>>16));s.WriteByte((byte)(v>>8));s.WriteByte((byte)v);}
    static void Png(string path,byte[] rgba){using var fs=File.Create(path);fs.Write(new byte[]{137,80,78,71,13,10,26,10});var ih=new byte[13];Put(ih,0,(uint)W);Put(ih,4,(uint)H);ih[8]=8;ih[9]=6;Chunk(fs,"IHDR",ih);var raw=new byte[H*(W*4+1)];for(int y=0;y<H;y++){raw[y*(W*4+1)]=0;Buffer.BlockCopy(rgba,y*W*4,raw,y*(W*4+1)+1,W*4);}using var ms=new MemoryStream();using(var z=new ZLibStream(ms,CompressionLevel.NoCompression,true))z.Write(raw);Chunk(fs,"IDAT",ms.ToArray());Chunk(fs,"IEND",Array.Empty<byte>());}
    static void Put(byte[] a,int i,uint v){a[i]=(byte)(v>>24);a[i+1]=(byte)(v>>16);a[i+2]=(byte)(v>>8);a[i+3]=(byte)v;}
    static void Rect(byte[] a,int x,int y,int w,int h,byte r,byte g,byte b){for(int yy=Math.Max(0,y);yy<Math.Min(H,y+h);yy++)for(int xx=Math.Max(0,x);xx<Math.Min(W,x+w);xx++){int i=(yy*W+xx)*4;a[i]=r;a[i+1]=g;a[i+2]=b;a[i+3]=255;}}
    static void Char(byte[] a,int ox,int oy,int row,int col,bool corvus){int sway=(col==1?-2:col==3?2:0); bool back=row==3||row==4; byte skin=corvus?(byte)176:(byte)150, skin2=corvus?(byte)128:(byte)108; // pixel clusters, 32x48 @ 3px
        int x=ox+59+sway,y=oy+18; Rect(a,x,y,30,27,(byte)(corvus?200:62),(byte)(corvus?190:80),(byte)(corvus?160:96)); // robe/cloak
        Rect(a,x+3,y-12,24,13,back?(byte)92:skin,back?(byte)74:skin2,back?(byte)62:skin2); // head/hair
        Rect(a,x-2,y+26,14,8,(byte)(corvus?55:72),(byte)(corvus?45:58),(byte)(corvus?42:48)); Rect(a,x+18,y+26,14,8,(byte)(corvus?55:72),(byte)(corvus?45:58),(byte)(corvus?42:48));
        if(corvus){Rect(a,x+4,y+1,22,3,225,220,196);Rect(a,x+12,y+4,6,20,226,197,50);Rect(a,x+30,y+2,3,30,110,76,38);Rect(a,x+33,y-1,7,4,190,160,80);}else{Rect(a,x-5,y+3,4,13,35,45,58);Rect(a,x+28,y+3,4,13,35,45,58);Rect(a,x-7,y+14,5,17,130,78,42);Rect(a,x+29,y+14,5,17,130,78,42);Rect(a,x+7,y+7,18,3,180,140,66);}
        if(!back){Rect(a,x+7,y+5,3,3,30,22,20);Rect(a,x+21,y+5,3,3,30,22,20);}
    }
    static void Main(string[] args){string root=args.Length>0?args[0]:".";Directory.CreateDirectory(Path.Combine(root,"Assets/_Game/Resources/NpcWalkSprites"));Directory.CreateDirectory(Path.Combine(root,"Assets/_Game/Resources/NpcSprites"));foreach(bool c in new[]{true,false}){string id=c?"npc_corvus":"npc_vaalara_wanderer_01";var a=new byte[W*H*4];for(int r=0;r<5;r++)for(int col=0;col<5;col++)Char(a,col*Cell,r*Cell,r,col,c);Png(Path.Combine(root,$"Assets/_Game/Resources/NpcWalkSprites/{id}_walk.png"),a);var b=new byte[128*128*4]; // base is a 128x128 crop-style canvas
            // upscale the centered character into the base canvas using the same palette contract
            Array.Clear(b,0,b.Length);for(int r=0;r<1;r++){var temp=new byte[W*H*4];Char(temp,0,0,0,0,c);for(int y=0;y<48;y++)for(int x=0;x<32;x++){int sx=59+x,sy=18+y,di=((70+y)*128+(48+x))*4,si=(sy*W+sx)*4;if(temp[si+3]!=0)for(int k=0;k<4;k++)b[di+k]=temp[si+k];}}PngBase(Path.Combine(root,$"Assets/_Game/Resources/NpcSprites/{(c?"npc_corvus":"npc_vaalara_wanderer_01")}.png"),b);}}
    static void PngBase(string path,byte[] rgba){int ow=W,oh=H; // temporarily encode 128 canvas with local encoder
        using var ms=new MemoryStream();ms.Write(new byte[]{137,80,78,71,13,10,26,10});var ih=new byte[13];Put(ih,0,128);Put(ih,4,128);ih[8]=8;ih[9]=6;Chunk(ms,"IHDR",ih);var raw=new byte[128*(128*4+1)];for(int y=0;y<128;y++){raw[y*(513)]=0;Buffer.BlockCopy(rgba,y*128*4,raw,y*513+1,512);}using var zms=new MemoryStream();using(var z=new ZLibStream(zms,CompressionLevel.NoCompression,true))z.Write(raw);Chunk(ms,"IDAT",zms.ToArray());Chunk(ms,"IEND",Array.Empty<byte>());File.WriteAllBytes(path,ms.ToArray());}
}
