using System;

namespace MAGMA2
{
    class Program
    {
        static string key = "ffeeddccbbaa99887766554433221100f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff";
        static uint a1 =0xfedcba98;     //старшая часть блока a
        static uint a0 = 0x76543210;    //младшая часть блока а
        static uint mask = 0b1111;

        static uint[] Encrypt(uint a1,uint a0,uint[] key)
        {
            uint numb;
            for (int i = 0; i < 24; i++)    //до 24 раунда ключи используются по возрастанию(с 1 по 8)
            {
                numb = a0;
                a0 = a1 ^ RoundFunction(numb, key[i % 8]);
                a1 = numb;
            }
            for (int i = 24; i < 31; i++)   //после 24 раунда ключи используются в обратном порядке(с 8 по 1)
            {
                numb = a0;
                a0 = a1 ^ RoundFunction(numb, key[7-(i % 8)]);
                a1 = numb;
            }
            a1 = a1 ^ RoundFunction(a0, key[0]);        //в последнем раунде блоки местами не меняются
            uint[] mas = { a1, a0 };
            return mas;
        }
        
        static uint [] Decrypt(uint a1, uint a0, uint[] key)
        {
            uint numb;
            a1 = a1 ^ RoundFunction(a0, key[0]);        //блоки местами не меняются

            for (int i = 30; i > 23; i--)    //до 24 раунда ключи используются в обратном порядке(с 8 по 1)
            {
                numb = a1;
                a1 = a0 ^ RoundFunction(numb, key[7 - (i % 8)]);
                a0 = numb;
            }
            for (int i = 23; i > -1; i--)   //после 24 раунда ключи используются по возрастанию(с 1 по 8)
            {
                numb = a1;
                a1 = a0 ^ RoundFunction(numb, key[i % 8]);
                a0 = numb;
            }
            uint[] mas = { a1, a0 };
            return mas;
        }
        static uint RoundFunction(uint a0,uint key)     //в фун-ции находится один раунд 
        {
            uint res = a0 + key;        // сложение a0 с раундовым ключом k операцией сложения 32-битовых чисел с отбрасыванием 33-го (старшего) бита (стандартное сложение uint32);
            res = BlockReplacement(res);    // блок замены
            uint shift = res >> 21;         //сохранение старших битов, чтобы не потерять их
            res=(res << 11)+shift;                //  циклический сдвиг влево на 11 бит(старшие 11 бит стали младшими)

            return res;
        }

        static uint BlockReplacement(uint x)            //блок замены 
        {
            uint block;
            uint[][] p = new uint[8][];
            uint newX = 0;

            p[0] = new uint[] { 12, 4, 6, 2, 10, 5, 11, 9, 14, 8, 13, 7, 0, 3, 15, 1 };
            p[1] = new uint[] { 6, 8, 2, 3, 9, 10, 5, 12, 1, 14, 4, 7, 11, 13, 0, 15 };
            p[2] = new uint[] { 11, 3, 5, 8, 2, 15, 10, 13, 14, 1, 7, 4, 12, 9, 6, 0 };
            p[3] = new uint[] { 12, 8, 2, 1, 13, 4, 15, 6, 7, 0, 10, 5, 3, 14, 9, 11 };
            p[4] = new uint[] { 7, 15, 5, 10, 8, 1, 6, 13, 0, 9, 3, 14, 11, 4, 2, 12 };
            p[5] = new uint[] { 5, 13, 15, 6, 9, 2, 12, 10, 11, 7, 8, 1, 4, 3, 14, 0 };
            p[6] = new uint[] { 8, 14, 2, 5, 6, 9, 1, 12, 15, 4, 11, 0, 13, 10, 3, 7 };
            p[7] = new uint[] { 1, 7, 14, 13, 0, 5, 8, 3, 4, 15, 10, 6, 9, 12, 11, 2 };

            for (int i = 0; i < p.Length; i++)
            {
                block = (x >> (i * 4)) & mask;
                newX += p[i][block] << (i * 4);
            }
            return newX;
        }
        static uint[] SplitKey(string key)      //разбиение ключа на 8 частей
        {
            uint[] masKey= new uint[8];         // массив ключей
            string keys="";
            for (int i = 0; i < 8; i++)         //цикл для ключей
            {
                for (int j = 0; j < 8; j++)
                {
                    keys += key[8 * i + j];     //по 8 символов
                }
                masKey[i] = Convert.ToUInt32(keys,16);
                keys = "";
            }
            return masKey;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Ключ (256 бит): " + key);
            foreach (uint i in SplitKey(key))               //проверка ключей
                Console.WriteLine("Ключ: "+ Convert.ToString(i,16));
            Console.WriteLine("(a1,a0)={0} , {1}",Convert.ToString(a1,16),Convert.ToString(a0,16));

            uint[] masEncrypt = Encrypt(a1, a0, SplitKey(key));
            uint encA1 = masEncrypt[0];
            uint encA0 = masEncrypt[1];
            Console.WriteLine("Шифрование: {0} {1}",Convert.ToString(encA1,16),Convert.ToString(encA0,16));

            uint[] masDecrypt = Decrypt(encA1, encA0, SplitKey(key));
            uint decA1 = masDecrypt[0];
            uint decA0 = masDecrypt[1];
            Console.WriteLine("Расшифрование: {0} {1}", Convert.ToString(decA1, 16), Convert.ToString(decA0, 16));

        }
    }
}
