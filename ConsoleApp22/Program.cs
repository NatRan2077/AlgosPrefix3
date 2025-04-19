using System.Globalization;

namespace algos2
{
    class Node
    {
        char symbol;
        List<Node> branches = new();
        bool isKey = false;

        // Метрики
        public int TotalNodesInSubtree()
        {
            int count = 1; // текущий узел
            foreach (var child in branches)
                count += child.TotalNodesInSubtree();
            return count;
        }

        public int LeafNodesCount()
        {
            if (branches.Count == 0) return 1;

            int count = 0;
            foreach (var child in branches)
                count += child.LeafNodesCount();
            return count;
        }

        public int InternalNodesCount(bool isRoot = true)
        {
            if (branches.Count == 0)
                return 0; // Лист не считается внутренним

            int count = isRoot ? 0 : 1; // Не считаем корень
            foreach (var child in branches)
                count += child.InternalNodesCount(false); // передаём дальше, что это не корень
            return count;
        }

        public int BranchingNodesCount()
        {
            int count = branches.Count > 1 ? 1 : 0;
            foreach (var child in branches)
                count += child.BranchingNodesCount();
            return count;
        }

        public double AvgBranchingFactor()
        {
            int branchingNodes = BranchingNodesCount();
            if (branchingNodes == 0) return 0;

            double totalBranches = CountBranchesInBranchingNodes();
            return totalBranches / branchingNodes;
        }

        private double CountBranchesInBranchingNodes()
        {
            double count = 0;

            if (branches.Count > 1)
                count += branches.Count;

            foreach (var child in branches)
                count += child.CountBranchesInBranchingNodes();

            return count;
        }

        public char Value
        {
            get { return symbol; }
            set { symbol = value; }
        }

        public bool IsKey
        {
            get { return isKey; }
            set { isKey = value; }
        }

        public bool HasChild(char value)
        {
            foreach (Node node in branches)
            {
                if (node.symbol == value)
                    return true;
            }
            return false;
        }

        public Node AddChild(char value)
        {
            Node child = new Node();
            child.symbol = value;
            branches.Add(child);
            return child;
        }

        public Node? GetChild(char value)
        {
            foreach (Node node in branches)
            {
                if (node.symbol == value)
                    return node;
            }
            return null;
        }

        public List<Node> GetDescendants()
        {
            List<Node> desc = new();
            foreach (Node child in branches)
            {
                desc.Add(child);
                desc.AddRange(child.GetDescendants());
            }
            return desc;
        }

        public List<string> GetWords(string parentWord = "")
        {
            List<string> pref = new();
            if (branches.Count == 0 && IsKey)
                return [parentWord];
            foreach (Node child in branches)
            {
                pref.AddRange(child.GetWords(parentWord + child.Value));
            }
            return pref;
        }
    }

    class Trie
    {
        Node root = new();

        // Метрики для всего дерева
        public int TotalCharacters { get; private set; }
        public int TotalWords => root.LeafNodesCount();
        public int InternalNodes => root.InternalNodesCount();
        public int BranchingNodes => root.BranchingNodesCount();
        public double AvgBranchingFactor => root.AvgBranchingFactor();

        public void Insert(string key, char value)
        {
            TotalCharacters += key.Length;
            Node node = root;
            for (int i = 0; i < key.Length; i++)
            {
                char ch = key[i];
                if (!node.HasChild(ch))
                    node = node.AddChild(ch);
                else
                    node = node.GetChild(ch);
            }
            node.Value = value;
            node.IsKey = true;
        }

        public Node? Lookup(string key)
        {
            Node? node = root;
            for (int i = 0; i < key.Length; i++)
            {
                char ch = key[i];
                if (!node.HasChild(ch))
                    return null;
                node = node.GetChild(ch);
            }
            return node.IsKey ? node : null;
        }

        public List<Node>? Search(string key)
        {
            List<Node>? results = new();
            if (key.Length == 0) return results;

            Node? node = root;
            for (int i = 0; i < key.Length; i++)
            {
                char ch = key[i];
                if (!node.HasChild(ch)) return null;
                node = node.GetChild(ch);
            }

            foreach (Node d in node.GetDescendants())
                if (d != null && d.IsKey)
                    results.Add(d);
            return results;
        }

        public List<string>? SearchWords(string key)
        {
            List<string>? results = new();
            if (key.Length == 0) return results;

            Node? node = root;
            for (int i = 0; i < key.Length; i++)
            {
                char ch = key[i];
                if (!node.HasChild(ch)) return null;
                node = node.GetChild(ch);
            }

            foreach (string d in node.GetWords(key))
                results.Add(d);
            return results;
        }

        public void PrintMetrics()
        {
            Console.WriteLine("\nМетрики префиксного дерева:");
            Console.WriteLine($"1. Общее количество символов: {TotalCharacters}");
            Console.WriteLine($"2. Количество слов (листовых вершин в дереве): {TotalWords}");
            Console.WriteLine($"3. Количество внутренних вершин: {InternalNodes}");
            Console.WriteLine($"4. Количество ветвлений (внутренних вершин из которых более одного пути): {BranchingNodes}");
            Console.WriteLine($"5. Среднее количество путей в вершинах ветвлений: {AvgBranchingFactor:F2}");
        }
    }

    class Program
    {
        public static List<string> ReadFile(string filename)
        {
            List<string> values = new();
            using (StreamReader file = new StreamReader(filename))
            {
                string? value;
                while ((value = file.ReadLine()) != null)
                {
                    values.Add(value.Split(' ')[1]);
                }
            }
            return values;
        }

        static void Main()
        {
            Trie tree = new();
            List<string> values = ReadFile("words.txt");

            Console.WriteLine($"Загружено слов: {values.Count}");

            foreach (string word in values)
                tree.Insert(word, word[word.Length - 1]);

            // Вывод метрик после загрузки
            tree.PrintMetrics();

            Console.Write("\nВведите слово для поиска: ");
            string? input;
            while ((input = Console.ReadLine()) == null)
                Console.Write("Попробуйте еще раз: ");

            List<string>? search = tree.SearchWords(input);
            if (search == null)
            {
                Console.WriteLine("Не найдено результатов");
                return;
            }

            Console.WriteLine($"Найдено слов: {search.Count}");
            foreach (var word in search)
                Console.WriteLine(word);
        }
    }
}
