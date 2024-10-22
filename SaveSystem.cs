using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Text.Json;
using System.Diagnostics;

namespace AutoClicker
{
    internal static class SaveSystem
    {

        public static void Save(ListBox saveBox, string saveName, List<KeyData> keyDatas) 
        {
            string filePath = "Saves/" + saveName + ".json";
            try
            {
                CreateSaveFolder();
                string json = JsonSerializer.Serialize(keyDatas);
                File.WriteAllText(filePath, json);                
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return;
            }
            GetSaves(saveBox);
        }
        public static void Load(ListBox keyListBox, ListBox saveBox)
        {
            if (saveBox.SelectedItem == null) return;
            
            string filePath = "Saves/" + ((Label)saveBox.SelectedItem).Content + ".json";
            if (!File.Exists(filePath)) return;
            string json = File.ReadAllText(filePath);
            List<KeyData>? keyDatas = JsonSerializer.Deserialize<List<KeyData>>(json);
            if (keyDatas == null) return;
            keyListBox.Items.Clear();
            keyDatas.ForEach(k =>
            {
                KeyControl kC = new();
                keyListBox.Items.Add(kC);
                kC.PasteData(VirtualKeyConverter.GetStringKey(k.VirtualKey), k.PressKey, k.DelayTime.ToString());
            });
            GetSaves(saveBox);
        }
        public static void Delete(ListBox saveBox)
        {
            if(saveBox.SelectedItem == null) return;
            string filePath = "Saves/" + ((Label)saveBox.SelectedItem).Content + ".json";
            if(File.Exists(filePath))
                File.Delete(filePath);
            GetSaves(saveBox);
        }

        public static void GetSaves(ListBox saveBox)
        {
            saveBox.Items.Clear();
            CreateSaveFolder();
            List<string> files = [.. Directory.GetFiles("Saves")];
            files.ForEach(f =>
            {
                Label l = new()
                {
                    Content = GetFileName(f),
                    Padding = new(0, 0, 0, 0)
                };
                saveBox.Items.Add(l);
            });
        }

        private static string GetFileName(string fileName)
        {
            return Path.GetFileName(fileName).Replace(".json", "");
        }

        private static void CreateSaveFolder()
        {
            if (!Directory.Exists("Saves"))
                Directory.CreateDirectory("Saves");
        }

    }

}
