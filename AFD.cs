using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace AFD_Minimal
{
    internal class AFD
    {
        public int nrStari { get; set; }
        public string[] alfabet { get; set; }
        public string stareInitiala { get; set; }
        public List<string> stariFinale { get; set; }
        public string literaStare { get; set; }
        public IDictionary<string, IDictionary<string, string>> tabelTranzitii { get; set; }
        public string[,] tabelPerechi { get; set; }
        public bool[,] matriceAdiacenta { get; set; }

        public AFD(string caleFisier)
        {
            using(StreamReader sr = new StreamReader(caleFisier))
            {
                //preluam nr de stari din fisier
                nrStari = int.Parse(sr.ReadLine());

                //preluam alfabetul din fisier
                alfabet = sr.ReadLine().Split(' ');

                //preluam starea initiala din fisier
                stareInitiala = sr.ReadLine();

                //preluam starile finale din fisier
                stariFinale = new List<string>();
                string[] buffer = sr.ReadLine().Split(' ');
                foreach(string stare in buffer)
                {
                     stariFinale.Add(stare);
                }

                //preluam tranzitiile din fisier
                tabelTranzitii = new Dictionary<string, IDictionary<string, string>> ();
                matriceAdiacenta = new bool[nrStari, nrStari];
                foreach(string caracter in alfabet)
                {
                    tabelTranzitii.Add(new KeyValuePair<string, IDictionary<string, string>>(caracter, new Dictionary<string, string>()));
                }
                while(!sr.EndOfStream)
                {
                    string[] tranzitie = sr.ReadLine().Split(' ');
                    string starea1 = tranzitie[0];
                    string starea2 = tranzitie[1];
                    string caracter = tranzitie[2];

                    tabelTranzitii[caracter][starea1] = starea2;
                    matriceAdiacenta[int.Parse(starea1.Substring(1)), int.Parse(starea2.Substring(1))] = true;
                    literaStare = starea1[0].ToString();

                }
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("AFD INITIAL" + Environment.NewLine);
                Console.ResetColor();
                AfiseazaAFD();
            }
           
        }
        private void DFSUtil(int stare, bool[] vizitat)
        {
            vizitat[stare] = true;
            for(int i = 0; i < nrStari; i++)
            {
                if (!vizitat[i] && matriceAdiacenta[stare, i])
                {
                    DFSUtil(i, vizitat);
                }
            }
        }

        private bool[] DFS(int stareInitiala)
        {
            bool[] vizitat = new bool[nrStari];
            DFSUtil(stareInitiala, vizitat);
            return vizitat;
        }
 
        public void MinimizeazaAFD()
        {
            //Stergem starile care nu sunt accesibile din starea initiala
            StergeStarileInaccesibile();
            ConstruiesteTabelPerechi();
           // afiseazaTabelPerechi();
            bool maiPutemMarcaPerechi = true;
            while (maiPutemMarcaPerechi)
            {
                maiPutemMarcaPerechi = false;
                for (int i = 1; i < nrStari; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (tabelPerechi[i, j] == null)
                        {
                            int stare1 = Math.Max(Delta(i, alfabet[0]), Delta(j, alfabet[0]));
                            int stare2 = Math.Min(Delta(i, alfabet[0]), Delta(j, alfabet[0]));

                            int stare3 = Math.Max(Delta(i, alfabet[1]), Delta(j, alfabet[1]));
                            int stare4 = Math.Min(Delta(i, alfabet[1]), Delta(j, alfabet[1]));

                            if (tabelPerechi[stare1, stare2] != null || tabelPerechi[stare3, stare4] != null)
                            {
                                tabelPerechi[i, j] = "X";
                                maiPutemMarcaPerechi = true;
                            }
                        }
                    }
                }
                
            }

            afiseazaTabelPerechi();

            List<List<int>> stariEchivalente = ConstruiesteStarileEchivalente();

            Console.ForegroundColor = ConsoleColor.Magenta;
            if (stariEchivalente.Count > 0)
            {
                IDictionary<string, string> hartaStari = ConstruiesteHartaStari(stariEchivalente);
                tabelTranzitii = ConstruiesteTabelTranzitiiMinimal(hartaStari);
                stariFinale = ConstruiesteStariFinale(hartaStari);
                stareInitiala = hartaStari[stareInitiala];
                
                Console.WriteLine("AFD MINIMAL" + Environment.NewLine);
                AfiseazaAFD();
            }
            else
            {
                Console.WriteLine("AFD-ul este minimal.");
            }
            Console.ResetColor(); 


        }

        private IDictionary<string, IDictionary<string, string>> ConstruiesteTabelTranzitiiMinimal(IDictionary<string, string> hartaStari)
        {
            
            IDictionary<string, IDictionary<string, string>> tabelTranzitiiNou = new Dictionary<string, IDictionary<string, string>>();
            foreach (string caracter in alfabet)
            {
                tabelTranzitiiNou.Add(new KeyValuePair<string, IDictionary<string, string>>(caracter, new Dictionary<string, string>()));
            }
            foreach (KeyValuePair<string, IDictionary<string, string>> kvp in tabelTranzitii)
            {
                foreach (KeyValuePair<string, string> kvp2 in kvp.Value)
                {
                    tabelTranzitiiNou[kvp.Key][hartaStari[kvp2.Key]] = hartaStari[kvp2.Value];
                }
            }
            return tabelTranzitiiNou;
        }

        private List<string> ConstruiesteStariFinale(IDictionary<string, string> hartaStari)
        {
            List<string> stariFinaleNoi = new List<string>();
            foreach(string stare in stariFinale)
            {
                if (!stariFinaleNoi.Contains(hartaStari[stare]))
                    stariFinaleNoi.Add(hartaStari[stare]);
            }
            return stariFinaleNoi;
        }

        private void StergeStarileInaccesibile()
        {
            bool[] stariAccesibile = DFS(int.Parse(stareInitiala.Substring(1)));
            bool existaStariInaccesibile = false;
            for (int i = 0; i < stariAccesibile.Length; i++)
            {
                if (!stariAccesibile[i])
                {
                    existaStariInaccesibile = true;
                    foreach (KeyValuePair<string, IDictionary<string, string>> kvp in tabelTranzitii)
                    {
                        List<string> stariInaccesibile = new List<string>();
                        foreach (KeyValuePair<string, string> kvp2 in kvp.Value)
                        {
                            if (kvp2.Key.Contains(i.ToString()) || kvp2.Value.Contains(i.ToString()))
                                stariInaccesibile.Add(kvp2.Key);
                        }
                        foreach (string stare in stariInaccesibile)
                        {
                            kvp.Value.Remove(stare);
                        }
                    }
                    nrStari--;
                    Console.WriteLine("Am scos " + literaStare + i + " pentru ca nu era accesibila din starea initiala." + Environment.NewLine);
                   
                }
            }
            if (existaStariInaccesibile)
                afiseazaTabelTranzitii();
            
        }

        private IDictionary<string, string> ConstruiesteHartaStari(List<List<int>> stariEchivalente)
        {
            IDictionary<string, string> hartaStari = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kvp in tabelTranzitii[alfabet[0]])
            {
                string stare1 = kvp.Key;
                bool areStareEchivalenta = false;
                foreach (List<int> grupStari in stariEchivalente)
                {
                    if (grupStari.Contains(int.Parse(stare1.Substring(1))))
                    {
                        string numeStareEchiv = "";
                        foreach (int stare in grupStari)
                        {
                            numeStareEchiv += literaStare + stare.ToString();
                        }
                        hartaStari.Add(stare1, numeStareEchiv);
                        areStareEchivalenta = true;
                    }
                }
                if (!areStareEchivalenta)
                {
                    hartaStari.Add(stare1, stare1);
                }
            }
            return hartaStari;
        }

        private List<List<int>> ConstruiesteStarileEchivalente()
        {
            List<List<int>> stariEchivalente = new List<List<int>>();
            for (int i = 1; i < nrStari; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (tabelPerechi[i, j] == null)
                    {
                        bool existaGrup = false;
                        foreach (List<int> grupStari in stariEchivalente)
                        {
                           
                            if (grupStari.Contains(i) && !grupStari.Contains(j))
                            {
                                grupStari.Add(j);
                                existaGrup = true;
                                break;
                            }
                            else if (grupStari.Contains(j) && !grupStari.Contains(i))
                            {
                                grupStari.Add(i);
                                existaGrup = true;
                                break;
                            }
                            else if(grupStari.Contains(j) && grupStari.Contains(i))
                            {
                                existaGrup = true;
                                break;
                            }

                        }
                        if (!existaGrup)
                            stariEchivalente.Add(new List<int> { i, j });
                    }
                }
            }
            afiseazaStariEchivalente(stariEchivalente);
            return stariEchivalente;
        }

        public int Delta(int stare, string caracter)
        {
            try
            {
                return int.Parse(tabelTranzitii[caracter][literaStare + stare.ToString()].Substring(1));
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                string errorMessage = "Automatul nu este complet.";
                Console.WriteLine(errorMessage);
                throw new Exception(errorMessage);
            }

        }

        private void ConstruiesteTabelPerechi()
        {
            tabelPerechi = new string[nrStari, nrStari];
           
                for (int i = 1; i < nrStari; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if ((EsteStareFinala(i) && !EsteStareFinala(j)) || (!EsteStareFinala(i) && EsteStareFinala(j)))
                        {
                            tabelPerechi[i, j] = "X";
                        }

                    }
                }

        }

        public bool EsteStareFinala(int stare)
        {
            return stariFinale.Contains(literaStare + stare.ToString());
        }

        public void afiseazaTabelTranzitii()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Tabel Tranztii : ");
            Console.Write("   |");
            foreach (KeyValuePair<string, string> kvp in tabelTranzitii[alfabet[0]])
            {
                 Console.Write(" {0:-10} |", kvp.Key);
            }
            
            Console.WriteLine(Environment.NewLine + "__________________________________________");
            foreach (KeyValuePair<string, IDictionary<string, string>> kvp in tabelTranzitii)
            {
                Console.Write(" " + kvp.Key + " |");
                foreach(KeyValuePair<string, string> kvp2 in kvp.Value)
                {
                    Console.Write(" {0:-10} |", kvp2.Value);
                }
          
                Console.WriteLine(Environment.NewLine + "__________________________________________");
               
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        public void afiseazaTabelPerechi()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Tabel Perechi : ");
            Console.Write("    |");
            for (int i = 0; i < nrStari; i++)
            {
                Console.Write(" " + literaStare + i);
            }
            Console.WriteLine(Environment.NewLine + "__________________________________________");
            for(int i = 0; i < nrStari; i++)
            {
                Console.Write(" " + literaStare + i + " |");

                for (int j = 0; j < nrStari; j++)
                    Console.Write(" " + ((i != j) ? tabelPerechi[i, j] : "]") + " ");

                Console.WriteLine(Environment.NewLine + "__________________________________________");
                
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        public void afiseazaStariEchivalente(List<List<int>> stariEchivalente)
        {
            if (stariEchivalente.Count > 0)
            {
                Console.WriteLine("Stari echivalente : ");
                foreach (List<int> grupStari in stariEchivalente)
                {
                    Console.Write("{");
                    foreach (int stare in grupStari)
                    {
                        Console.Write(" " + literaStare + stare + " ");
                    }
                    Console.Write("}" + Environment.NewLine);
                }
                Console.WriteLine();
            }
        }

        public void afiseazaStareInitiala()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Stare initiala : " + stareInitiala + Environment.NewLine);
            Console.ResetColor();
            
        }

        public void afiseazaStariFinale()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Stari finale : { ");
            foreach(string stare in stariFinale)
            {
                Console.Write(stare + " ");
            }
            Console.Write("}" + Environment.NewLine);
            Console.ResetColor();
        }

        public void AfiseazaAFD()
        {
            afiseazaTabelTranzitii();
            afiseazaStareInitiala();
            afiseazaStariFinale();
            Console.WriteLine();
            
        }

    }
}
