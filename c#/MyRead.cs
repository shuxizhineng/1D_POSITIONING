using System.IO;
using System.Text;
using System;


namespace Read
{
    public class Myread
    {
        public byte[] byData=new byte[10000000];

        public int dataNum=0;

        public Myread(string path) 
        {
            try
            {
                string s = System.IO.File.ReadAllText(path);
                string[] nums = s.Replace("\r", "").Replace("\n", " ").Split(' ');
                int offset=0;
                byte[] tmp=new byte[10000000];
                byte[] tmp2=new byte[10000000];

                // 计算数据偏移
                var seek=0;
                for (var i=0;i<nums.Length;i++)
                {
                    if (nums[i]=="F5")
                    {
                        seek=i;
                        break;
                    }
                }
                for (int i=seek;i<nums.Length;i++)
                {
                    if (nums[i]=="\\par")
                    {
                        offset++;
                    }
                    else
                    {
                        Console.WriteLine(nums[i]);
                        try{
                            byte b = System.Convert.ToByte(nums[i], 16);
                            tmp[i-1-offset]=b;
                        }
                        catch
                        {
                            continue;
                        }
                        // Console.Write(b);
                    }
                    // byte b = Byte.Parse(nums[i],System.GlobalizationNumberStyle.HexNumber);
                }

                dataNum=nums.Length-offset;
                Array.Copy(tmp, byData, nums.Length-offset);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }

}

