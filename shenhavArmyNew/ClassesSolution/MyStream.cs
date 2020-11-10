using System.IO;

//extended StreamReader with Pos function and current Row.
public class MyStream : StreamReader
{
    private uint _pos;
    public uint curRow;
    private uint previousRowSaved;
    public MyStream(string path, System.Text.Encoding enc) : base(path, enc)
    {
        this.curRow = 0;
        this.previousRowSaved = 0;
    }
    public override string ReadLine()
    {
        char current;
        int i;
        string line = null;
        while ((i = base.Read()) != -1)
        {
            
            current = (char)i;
            _pos++;
            if (IsFeed(current))
            {
                if(i==13)
                {
                    this.curRow++;
                }
                if ((i = base.Peek()) != -1)
                {
                    if (!IsFeed((char)i))
                    {
                        break; //avoid.
                    }
                }
            }
            else line += current;
        }
        return line;
    }
    private bool IsFeed(char c)
    {
        
        if (c == '\r' || c == '\n')
        {
            return true;
        }
        return false;
    }
    public uint Seek(uint pos)
    {
        this.DiscardBufferedData();
        this.BaseStream.Seek(pos, SeekOrigin.Begin);
        _pos = pos;
        this.curRow = this.previousRowSaved;
        return pos;
    }
    public uint Pos
    {
        get
        {
            this.previousRowSaved = curRow;
            return _pos;
        }
    }
    
}

