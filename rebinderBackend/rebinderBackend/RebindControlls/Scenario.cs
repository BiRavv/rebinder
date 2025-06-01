using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using rebinderBackend.FrontendConnection;

namespace rebinderBackend.RebindControls
{
    public class Scenario
    {
        private readonly List<IBind> _binds = [];
        public bool IsActive { get; private set; } = false;
        public string Name { get; private set; } = string.Empty;
        
        public static List<Scenario> AllScenarios = new List<Scenario>();

        
        // This is very important, so names don't clash.
        private static readonly List<string> Names = new List<string>();
        private static string ValidateName(string ogName)
        {
            int overflow = 0;
            while (Names.Contains(ogName))
            {
                overflow++;
                ogName += " copy";
                if (overflow > 100) throw new Exception("Scenario name overflow");
            }
            Names.Add(ogName);
            return ogName;
        }

        /// <summary>
        /// This can add binds to the scenario.
        /// </summary>
        /// <param name="bind">Any IBind</param>
        /// <returns>The id of the bind in reference to this scenario</returns>
        public int AddBind(IBind bind)
        {
            _binds.Add(bind);
            return _binds.Count-1;
        }
        public void RemoveBind(int index)
        {
            _binds.RemoveAt(index);
        }

        public void SetActive(bool sate)
        {
            IsActive = sate;
            foreach (IBind bind in _binds)
            {
                if (IsActive) bind.Start();
                else bind.Stop();
            }
        }
        
        /// <summary>
        /// Creates a scenario. A scenario holds many binds, and controls them. The listener will listen for "scenario@[Name of this scenario]"
        /// </summary>
        /// <param name="name">The name of the scenario. It will be validated, so it's not sure that it will be exactly the arg. 'Name' variable is public to get.</param>
        public Scenario(string name)
        {
            Name = ValidateName(name);
            AllScenarios.Add(this);
            Console.WriteLine("New scenario: " + Name);
            
            // Toggle switch for active
            Fetch.AddListener(body =>
            {
                if (body != $"scenario@{Name}") return null;
                
                IsActive = !IsActive;
                foreach (IBind bind in _binds)
                {
                    if (IsActive) bind.Start();
                    else bind.Stop();
                }
                
                return null; // Returns any
            });
            // Stops scenario
            Fetch.AddListener(body =>
            {
                if (body != $"stop_scenario@{Name}") return null;
                
                IsActive = false;
                foreach (IBind bind in _binds)
                {
                    bind.Stop();
                }
                
                return null; // Returns any
            });
            // Gives back all the binds the scenario has
            Fetch.AddListener(body =>
            {
                if (body != $"scenario_binds@{Name}") return null;

                Console.WriteLine(body);
                
                string responseBinds = "";
                for (int i = 0; i < _binds.Count; i++)
                {
                    responseBinds += i + "&" + _binds[i].ToFrontendData() + "/";
                }
                
                return responseBinds;
            });
            
            Fetch.AddListener(body =>
            {
                if (!body.StartsWith($"change_bind@{Name}")) return null;

                string toChange = body.Split(new[] { '@' })[2];

                try
                {
                    IBind bind = _binds[Int32.Parse(toChange.Split('&')[0])];


                    if (toChange.Split('&')[1] == "1")
                    {
                        StringMap stringMap = (StringMap) bind;
                        stringMap.fromKey = (Keys)Int64.Parse(toChange.Split('&')[2].Split('>')[0]);
                        stringMap.textToPaste = toChange.Split('&')[2].Split('>')[1];
                    }
                    else if (toChange.Split('&')[1] == "0")
                    {
                        KeyMap keyMap = (KeyMap)bind;
                        keyMap.fromKey = (Keys)Int64.Parse(toChange.Split('&')[2].Split('>')[0]);
                        keyMap.toKeys = new List<Keys>();
                        foreach (string keyString in toChange.Split('&')[2].Split('>')[1].Split(';'))
                        {
                            if (keyString == "" || keyString == null) continue;
                            Console.WriteLine((Keys)Int64.Parse(keyString));
                            keyMap.toKeys.Add(((Keys)Int64.Parse(keyString))); ;
                            Console.WriteLine(keyMap.toKeys.Count);
                        }
                        _binds[Int32.Parse(toChange.Split('&')[0])] = keyMap;
                        Console.WriteLine("keys : "+toChange.Split('&')[2].Split('>')[1]);
                    }
                }
                catch (Exception e)
                {

                    if (toChange.Split('&')[1] == "1")
                    {
                        
                        Keys fromKey = (Keys)Int64.Parse(toChange.Split('&')[2].Split('>')[0]);
                        string textToPaste = toChange.Split('&')[2].Split('>')[1];
                        AddBind(new StringMap(fromKey, textToPaste));
                        Console.WriteLine(_binds.Count);
                    }
                    else if (toChange.Split('&')[1] == "0")
                    {
                       
                        Keys fromKey = (Keys)Int64.Parse(toChange.Split('&')[2].Split('>')[0]);
                        AddBind(new KeyMap(fromKey, new List<Keys>() { }));
                    }

                }

                return null;
            });
        }
    }
}