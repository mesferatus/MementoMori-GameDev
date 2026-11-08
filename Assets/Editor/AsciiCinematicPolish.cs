using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using MementoMori.Dialogue;
using UnityEditor;
using UnityEngine;

/// <summary>Authored character compositions. Does not change narrative triggers or dialogue.</summary>
public static class AsciiCinematicPolish
{
    const int W = 104, H = 40;
    const string Stone = "827994", Ink = "B8A0DF", Light = "F0E6FF", Moon = "A9DBE6", Gold = "E5C183", Hair = "B39CD1";
    sealed class Drawing
    {
        readonly char[,] cells = new char[H,W];
        readonly string[,] colors = new string[H,W];
        public Drawing() { for(int y=0;y<H;y++)for(int x=0;x<W;x++)cells[y,x]=' '; }
        public void Put(int x,int y,string art,string color=Ink)
        {
            var lines=art.Replace("\r","").Split('\n');
            for(int j=0;j<lines.Length;j++)for(int i=0;i<lines[j].Length;i++)
                if(x+i>=0&&x+i<W&&y+j>=0&&y+j<H)
                {cells[y+j,x+i]=lines[j][i];colors[y+j,x+i]=color;}
        }
        public void Pixel(int x,int y,char glyph,string color)
        {
            if(x<0||x>=W||y<0||y>=H)return;
            cells[y,x]=glyph;
            colors[y,x]=color;
        }
        public void Fade(int phase)
        {
            for(int y=0;y<H;y++)for(int x=0;x<W;x++)
                if((x*3+y*5)%11<phase)cells[y,x]=' ';
        }
        public override string ToString()
        {
            var s=new StringBuilder();
            for(int y=0;y<H;y++)
            {
                string ink=null;
                for(int x=0;x<W;x++)
                {
                    var next=colors[y,x]??Stone;
                    if(next!=ink){if(ink!=null)s.Append("</color>");s.Append("<color=#").Append(next).Append('>');ink=next;}
                    s.Append(cells[y,x]);
                }
                s.Append("</color>");if(y<H-1)s.Append('\n');
            }
            return s.ToString();
        }
    }

    // The portraits are sampled from the approved in-game sprites. This keeps
    // Melantha's hair and clothing, Poe's silhouette, and Andrealphus's
    // feathers recognizable even when the stage is made entirely of glyphs.
    static readonly Dictionary<string,Sprite> portraitSprites=new Dictionary<string,Sprite>();
    static readonly Dictionary<Texture2D,Color32[]> portraitPixels=new Dictionary<Texture2D,Color32[]>();
    static Sprite Portrait(string path,string name)
    {
        var key=path+"#"+name;
        if(!portraitSprites.TryGetValue(key,out var sprite))
        {
            sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault(s=>s.name==name);
            if(sprite==null)throw new InvalidOperationException("Missing approved portrait sprite "+key);
            portraitSprites[key]=sprite;
        }
        return sprite;
    }
    static Color32[] Pixels(Texture2D texture)
    {
        if(portraitPixels.TryGetValue(texture,out var result))return result;
        if(texture.isReadable)result=texture.GetPixels32();
        else
        {
            var active=RenderTexture.active;
            var rt=RenderTexture.GetTemporary(texture.width,texture.height,0,RenderTextureFormat.ARGB32);
            Graphics.Blit(texture,rt);
            RenderTexture.active=rt;
            var copy=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            copy.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);
            copy.Apply();
            result=copy.GetPixels32();
            UnityEngine.Object.DestroyImmediate(copy);
            RenderTexture.active=active;
            RenderTexture.ReleaseTemporary(rt);
        }
        portraitPixels[texture]=result;
        return result;
    }
    static void Stamp(Drawing d,Sprite sprite,int left,int top,int width,int height)
    {
        var tex=sprite.texture;
        var src=Pixels(tex);
        var rect=sprite.rect;
        int rx=(int)rect.x,ry=(int)rect.y,rw=(int)rect.width,rh=(int)rect.height;
        int minX=rw,minY=rh,maxX=-1,maxY=-1;
        for(int sy=0;sy<rh;sy++)for(int sx=0;sx<rw;sx++)
        {
            var c=src[(ry+sy)*tex.width+rx+sx];
            if(c.a<48)continue;
            minX=Math.Min(minX,sx);maxX=Math.Max(maxX,sx);
            minY=Math.Min(minY,sy);maxY=Math.Max(maxY,sy);
        }
        if(maxX<minX)return;
        const string density="@%#*+=-:.";
        for(int gy=0;gy<height;gy++)for(int gx=0;gx<width;gx++)
        {
            int sx=minX+Math.Min(maxX-minX,(int)((gx+.5f)*(maxX-minX+1)/width));
            int sy=maxY-Math.Min(maxY-minY,(int)((gy+.5f)*(maxY-minY+1)/height));
            var c=src[(ry+sy)*tex.width+rx+sx];
            if(c.a<60)continue;
            float l=(.27f*c.r+.59f*c.g+.14f*c.b)/255f;
            int index=Math.Min(density.Length-1,Math.Max(0,(int)(l*(density.Length-1))));
            var name=sprite.name;
            bool witch=name.StartsWith("Melantha")||name.StartsWith("melantha")||name=="idle_front";
            bool cat=name.StartsWith("Poe");
            string color=witch
                ?(l>.72f?Light:c.r>c.b*1.18f&&l>.4f?Gold:l<.23f?Stone:l<.48f?Ink:Hair)
                :cat
                    ?(c.r>c.b*1.25f&&l>.34f?Gold:l>.65f?Light:l<.24f?Stone:Hair)
                    :(c.r>c.b*1.18f&&l>.36f?Gold:l>.7f?Light:l<.23f?Stone:l<.48f?"758CCB":Moon);
            d.Pixel(left+gx,top+gy,density[index],color);
        }
    }

    [MenuItem("Memento Mori/Cinematics/Apply authored cinematic polish")]
    public static void Apply()
    {
        foreach(var id in new[]{"C01","C02","C03","L01","C_POE","A1","L08","M02_WATER","C_BLACK","A2","M07","FINAL"})
        {
            var s=AssetDatabase.LoadAssetAtPath<IllustratedSequence>("Assets/Personagens/Resources/Cinematics/"+id+".asset");
            if(s==null)throw new InvalidOperationException("Missing cinematic "+id);
            Undo.RecordObject(s,"Cinematic composition "+id);
            for(int b=0;b<s.beats.Length;b++)
            {
                int beat=b;
                s.beats[b].asciiFrames=Enumerable.Range(0,8).Select(f=>Compose(id,beat,f,false)).ToArray();
                if(id=="M07")foreach(var variant in s.beats[b].choiceVariants)
                    variant.asciiFrames=Enumerable.Range(0,8).Select(f=>Compose(id,beat,f,variant.optionId=="approach")).ToArray();
            }
            // Approval follows visual review; generating frames is not artistic acceptance.
            s.finalArtwork=false;
            EditorUtility.SetDirty(s);
        }
        AssetDatabase.SaveAssets();
    }

    static string Compose(string id,int beat,int frame,bool approach)
    {
        var d=new Drawing();
        bool blink=frame==3;
        switch(id)
        {
            case "C01":
                Ritual(d,28,22,frame);
                Hand(d,12+(beat==0?Math.Min(frame,3):3-Math.Min(frame,3)),7);
                Candle(d,79,12,frame);
                d.Put(30,4,".          :          .",Stone);
                d.Put(32,8,"       .       .",Stone);
                if(beat>0)d.Put(53,19,".  .   .",Light);
                break;
            case "C02": case "FINAL":
                Room(d);
                if(beat==0)
                {
                    d.Put(13,18,@"  .---.____
 / /|||\   \
 |  - -|   |
  \_._/____|
 /~~~~~~~~~\
 | ~~~~~~~ |",Hair);
                    if(frame>3)Melantha(d,17,12,false,blink,true);
                }
                else
                {
                    Melantha(d,40,11,false,blink,false);
                    if(id=="FINAL")d.Put(68,25,frame<4?" .-.-.\n(  /  )\n '-'-'":" .-.-.\n( /|  )\n '-'-'",Light);
                    else d.Put(59,17,frame<4?"  .":".  ",Light);
                }
                break;
            case "C03":
                Room(d);Ritual(d,30,23,frame);Candle(d,49,9,frame);
                if(beat>0&&frame>2)d.Fade((frame-2)*2);
                break;
            case "L01":
                Masonry(d);Ritual(d,36,24,frame);
                Hand(d,9,10);
                d.Put(71,8,@"|  :    |   |
|  :    |   |
|  :    |   |
|  :    |   |
|  :    |   |
|  :    |   |
|  '----'   |
| /      \  |
|/        \ |",Stone);
                if(beat>0)d.Put(81,10,frame%4<2?" .   )":"  .  ))",Moon);
                break;
            case "C_POE":
                Masonry(d);Melantha(d,12,10,false,blink,false);
                Poe(d,65-(beat==0?Math.Min(frame,5):5),21,blink,beat>0);
                if(beat>0)Hand(d,36,16);
                d.Put(9,36,"____..___________..____________________..____________..____________________",Stone);
                break;
            case "A1": case "A2":
                if(id=="A1")Masonry(d);else MoonGarden(d,frame);
                Stamp(d,id=="A2"
                    ?Portrait("Assets/Personagens/Melantha/Lunar/Melantha_Lunar_Cutscene.png",
                        "Melantha_Lunar_Cutscene_r0_c"+(frame%6))
                    :Portrait("Assets/Personagens/Melantha/Sprites/idle_front.png","idle_front"),
                    6,3,43,36);
                Stamp(d,Portrait("Assets/Personagens/Andrealphus/Sprites/Andrealphus_Bird.png",
                    "Andrealphus_Bird_idle_float_"+(frame%6)),56,2,44,37);
                d.Put(55,32,".      '      .      '      .",Moon);
                if(frame<3)d.Fade(3-frame);
                break;
            case "L08":
                Masonry(d);Portal(d,52,3,beat>0,frame);
                Melantha(d,15,13,false,blink,false);Poe(d,35,27,blink,true);
                break;
            case "M02_WATER":
                MoonGarden(d,frame);
                if(beat==0)Melantha(d,22,9,true,blink,false);
                else
                {
                    d.Put(43+frame/3,23,@"   .-||||-.
  /||/||\||\
  ||  o o ||
  ||\ ._ /||
~~'~~\__/~~'~~",Hair);
                    d.Put(36+frame/3,29,frame%2==0?"~   --(________)--   ~":"  ~ --(__________)-- ~",Moon);
                }
                Poe(d,7,24,blink,true);
                break;
            case "C_BLACK":
                d.Put(52,2,@"         .-==o==-.
      .-'  / | \  '-.
    .'  .-'  |  '-.  '.
   /  .'     |     '.  \
  /  /               \  \
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |                 |  |
 |  |_________________|  |
  \_____________________/
       /           \
      /_____________\",Stone);
                Melantha(d,18,10,true,blink,false);Melantha(d,60,8,true,beat==0?blink:frame==6,false);
                if(beat>0)d.Put(66,19,". . . .",Moon);
                break;
            case "M07":
                Masonry(d);Ritual(d,29,24,frame);
                d.Put(46,5,beat==0?@"     /\
    /  \
   / /\ \
   \ \/ /
    \  /
     \/":@"     /\
  . /| \. 
   / |  \
   \ |  /
  ' \| / '
     \/",Light);
                Hand(d,7,12);Poe(d,78-(approach?frame:0),25,blink,true);
                break;
        }
        return d.ToString();
    }

    static void Melantha(Drawing d,int x,int y,bool lunar,bool blink,bool seated)
    {
        if(x>=0)
        {
            var sprite=seated
                ?Portrait("Assets/Personagens/Melantha/Sprites/melantha_wake_up_04.png","melantha_wake_up_04_0")
                :lunar
                    ?Portrait("Assets/Personagens/Melantha/Lunar/Melantha_Lunar_Cutscene.png",
                        blink?"Melantha_Lunar_Cutscene_r0_c1":"Melantha_Lunar_Cutscene_r0_c0")
                    :Portrait("Assets/Personagens/Melantha/Sprites/idle_front.png","idle_front");
            Stamp(d,sprite,x,y,30,26);
            return;
        }
        d.Put(x,y,@"      .-||||-.
    .'/|/||\|\|'.
   /:|||/||\||||:\
  /:||/      \|||:\
  |:| |      | ||:|
  |:| | .  . | ||:|
  |:|  \_.._/  ||:|
  |||  _/\/\_  ||||
  ||| /:\__/:\ ||||
  |||/|: ::  |\||||
  ||/ |: ::  | \|||
  |/  /--<>--\  \||
  /__/ / || \ \__\
   () /  ||  \ ()
     /:__||___\
    /:/ /||\ \ \
   /_/ /_||_\ \_\
   \_v_v_v_v_v_v/
      |__| |__|
      /__| |__\",Hair);
        d.Put(x+8,y+4,blink?"-  -":"o  o",Light);
        d.Put(x+9,y+7,lunar?"(c)":"<> ",lunar?Moon:Gold);
        d.Put(x+8,y+11,lunar?"* c *":"-<>-",lunar?Moon:Ink);
        if(seated)d.Put(x+3,y+17,@"(____/  \____)",Ink);
    }
    static void Poe(Drawing d,int x,int y,bool blink,bool sitting)
    {
        if(x>=0)
        {
            Stamp(d,Portrait("Assets/Personagens/Poe/Sprites/Poe_Reactions.png",
                sitting?"Poe_Reactions_sit_side_1":"Poe_Reactions_alert_4"),x,y,21,15);
            return;
        }
        d.Put(x,y,@" /\       /\
/  '.---.'  \
|  /     \  |
|           |
 \   \_/   /
  '._/ \_.'
   /     \
  /       \    _
 (|       |)  / )
 (_|_____|_)_/ /",Hair);
        d.Put(x+3,y+3,blink?"-   -":"o   o",Gold);
        if(!sitting)d.Put(x+2,y+9,@"_/ /  \_\__",Ink);
    }
    static void Peacock(Drawing d,int x,int y,int frame)
    {
        if(x>=0)
        {
            Stamp(d,Portrait("Assets/Personagens/Andrealphus/Sprites/Andrealphus_Bird.png",
                "Andrealphus_Bird_idle_float_"+(frame%6)),x,y,38,31);
            return;
        }
        d.Put(x,y,@"          \ | /
           \|/
          .-^-.
         / o  >
         \   /
          |:|
       .-'/:'-.
      / / | \ \
     / / /|\ \ \
    / ()/ | \() \
   / / / /|\ \ \ \
  / ()/ / | \ \() \
 / / / () | () \ \ \
/ ()/ / / | \ \ \() \
\ / / () /|\ () \ \ /
 V / / / /|\ \ \ \ V
  V / ()/ | \() \ V
   V / / /|\ \ \ V
    V / / | \ \ V
     V / /|\ \ V
      V / | \ V
       V /|\ V
        V | V
          |
         / \
        /   \","758CCB");
        d.Put(x+11,y+3,"o",Gold);
        if(frame>=4)d.Put(x+21,y+9,@"\   .-()
 \-' .-()
  '--'",Moon);
        d.Put(x+7,y+10,"()",Gold);d.Put(x+14,y+13,"()",Gold);d.Put(x+9,y+18,"()",Moon);
    }
    static void Hand(Drawing d,int x,int y)
    {
        d.Put(x,y,@"          __   __
         / /  / /
    __  / /__/ /__
   / / / /  / /  /
  / /_/ /  / /  /
 /     /__/ /__/
|             /
|    .---.   /
 \   '---'  /
  \        /
   |      |
  /|      |\
 / |______| \
/___/____\___\",Light);
    }
    static void Candle(Drawing d,int x,int y,int frame)
    {
        d.Put(x+3,y,frame%3==0?" )\n( )":" (\n( )",Gold);
        d.Put(x,y+2,@"   |'|
   | |
   | |
   | |
 __|_|__
(_______)
   /_\",Gold);
    }
    static void Ritual(Drawing d,int x,int y,int frame)
    {
        d.Put(x,y,@"           .---o---.
      .---'   /|\   '---.
   .-'       / | \       '-.
  /    .----/--+--\----.    \
 (----+----/---|---\----+----)
  \    \  /    |    \  /    /
   '-.  \/_____|_____\/  .-'
      '--.     |     .--'
          '----o----'",Ink);
        d.Put(x+15,y+3,frame%4<2?"*":"+",Light);
    }
    static void Masonry(Drawing d)
    {
        d.Put(3,2,@"|________|________|________|________|________|________|________|________|________|________|
|___|________|________|________|________|________|________|________|________|________|____|
|________|________|________|________|________|________|________|________|________|________|",Stone);
        d.Put(4,34,"____/_________/_________/_________/_________/_________/_________/_________/_________/____",Stone);
        d.Put(2,37,"__/_________/_________/_________/_________/_________/_________/_________/_________/______",Stone);
    }
    static void Room(Drawing d)
    {
        Masonry(d);
        d.Put(39,5,@"     .-=====--.
   .' /  |  \ '.
  /| / * | . \ |\
  ||/____|____\||
  || .   |    |||
  ||_____|____|||
  '============='",Moon);
        d.Put(7,13,@"o================o
|/\_/\_/\_/\_/\_/|
| .------------. |
| |            | |
| '------------' |
|~~~~~~~~~~~~~~~~|
|~  ~~~  ~~~  ~~~|
|~~  ~~~  ~~~  ~~|
|~~~  ~~~  ~~~  ~|
|~  ~~~  ~~~  ~~~|
|~~  ~~~  ~~~  ~~|
|~~~  ~~~  ~~~  ~|
|================|
o================o
 |              |",Ink);
        d.Put(76,15,@"        /'
       /'
  ____/_______
 /___/|\______\
 |===|||====| |
 |___|||____| |
o=============o
|  [_____]    |
|             |
|             |",Gold);
        d.Put(35,32,@".------------------------------.
| /\    .----o----.      /\    |
| \/---(----/|\----)-----\/    |
|        '--_o_--'             |
'------------------------------'",Ink);
    }
    static void Portal(Drawing d,int x,int y,bool open,int frame)
    {
        d.Put(x,y,@"          .---o---.
       .-'  / | \  '-.
     .'  o /  |  \ o '.
    /  .--/---o---\--. \
   /  /               \ \
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |  |                 | |
  |__|_________________|_|",Stone);
        if(open){d.Put(x+8,y+7,@".   *     .
     .
  .     *
     |
    / \
   /   \",Moon);d.Put(x+7+frame/3,y+15,".   '   .",Light);}
        else d.Put(x+11,y+11,@" /\
<||>
 \/",Gold);
    }
    static void MoonGarden(Drawing d,int frame)
    {
        d.Put(79,3,"  .-''-.\n / .  . \\\n|   .    |\n \\  .  /\n  '----'",Moon);
        d.Put(4,5,".         *            .                 .          *",Stone);
        d.Put(3,21,"____.._____.._____..__                     __..____..____..___.._____..______",Stone);
        d.Put(6,33,"     ~~~           ~~~~~       ~~~                ~~~~~          ~~~",Moon);
        d.Put(19+frame%3,36,"~~~~          ~~~                ~~~~~          ~~~            ~~~~",Moon);
        d.Put(78,25,"   .-.\n--( * )--\n   '-'\n ~(___)~",Ink);
        d.Put(62,29,"  .-.\n-(*)-\n ~(_)~",Light);
    }
}
