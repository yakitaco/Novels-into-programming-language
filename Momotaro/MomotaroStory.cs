using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MomotaroStory
{
    // --- 1. レコード (Record) ---
    public record KibiDango(int PowerBoost, string Description = "日本一のきびだんご");

    public record Treasure(string Name, int Value, string Description);

    public record Peach(string Size)
    {
        public Momotaro CutOpen()
        {
            Console.WriteLine("「ぱっかーん！」");
            Console.WriteLine($"なんと、{Size}桃の中から元気な男の子が飛び出しました！");
            return new Momotaro(hp: 100, baseAttack: 30);
        }
    }

    // --- 2. インターフェース (Interface) ---
    public interface IAttacker
    {
        int AttackPower { get; }
        void Attack(Character target);
    }

    // --- 3. 抽象クラス (Abstract Class) & 継承 (Inheritance) ---
    public abstract class Character
    {
        public string Name { get; protected set; }
        public int Hp { get; set; }
        public bool IsAlive => Hp > 0;

        protected Character(string name, int hp)
        {
            Name = name;
            Hp = hp;
        }

        public virtual void TakeDamage(int damage)
        {
            Hp -= damage;
            Console.WriteLine($"{Name}は {damage} のダメージを受けた！ (残りHP: {Math.Max(0, Hp)})");
            if (!IsAlive)
            {
                Console.WriteLine($"【討伐】{Name} は倒れた...");
            }
        }
    }

    // --- 4. 派生クラス (Derived Classes) ---
    public class Villager : Character
    {
        public Villager(string name) : base(name, hp: 30) { }

        public void DoChore(string location, string action)
        {
            Console.WriteLine($"{Name}は {location} へ {action} に行きました。");
        }

        // 宝物を受け取って喜ぶメソッド
        public void Rejoice(Treasure treasure)
        {
            Console.WriteLine($"{Name}「おお、なんと素晴らしい『{treasure.Name}』じゃ！よくやった、桃太郎や！」");
        }
    }

    public class Momotaro : Character, IAttacker
    {
        public int AttackPower { get; private set; }
        private List<KibiDango> _inventory = new();

        // 手に入れた宝物を格納するリスト
        public List<Treasure> Treasures { get; } = new();

        public delegate void CompanionJoinedHandler(string companionName);
        public event CompanionJoinedHandler? OnCompanionJoined;

        public Momotaro(int hp, int baseAttack) : base("桃太郎", hp)
        {
            AttackPower = baseAttack;
        }

        public void Obtain(KibiDango dango)
        {
            _inventory.Add(dango);
            Console.WriteLine($"{Name} は『{dango.Description}』を手に入れた！");
        }

        public bool GiveKibiDango(Animal companion)
        {
            if (_inventory.Count > 0)
            {
                var dango = _inventory[0];
                _inventory.RemoveAt(0);
                companion.Eat(dango);

                OnCompanionJoined?.Invoke(companion.Name);
                return true;
            }
            return false;
        }

        public void Attack(Character target)
        {
            Console.WriteLine($"{Name} の刀による攻撃！");
            target.TakeDamage(AttackPower);
        }
    }

    public enum AnimalType { Dog, Monkey, Pheasant }

    public class Animal : Character, IAttacker
    {
        public AnimalType Type { get; }
        public int AttackPower { get; private set; }

        public Animal(AnimalType type) : base(GetAnimalName(type), 50)
        {
            Type = type;
            AttackPower = 10;
        }

        private static string GetAnimalName(AnimalType type) => type switch
        {
            AnimalType.Dog => "犬",
            AnimalType.Monkey => "猿",
            AnimalType.Pheasant => "雉",
            _ => throw new ArgumentException("未知の動物です")
        };

        public void Eat(KibiDango dango)
        {
            AttackPower += dango.PowerBoost;
            Console.WriteLine($"{Name} はきびだんごを食べた！力が {dango.PowerBoost} アップした！");
        }

        public void Attack(Character target)
        {
            string attackMsg = Type switch
            {
                AnimalType.Dog => "噛みつき攻撃",
                AnimalType.Monkey => "引っかき攻撃",
                AnimalType.Pheasant => "つつき攻撃",
                _ => "体当たり"
            };
            Console.WriteLine($"{Name} の {attackMsg}！");
            target.TakeDamage(AttackPower);
        }
    }

    public class Oni : Character, IAttacker
    {
        public int AttackPower { get; private set; }

        public Oni(string name, int hp, int attackPower) : base(name, hp)
        {
            AttackPower = attackPower;
        }

        public void Attack(Character target)
        {
            Console.WriteLine($"{Name} の金棒による強烈な一撃！");
            target.TakeDamage(AttackPower);
        }

        // ランダムに宝物をドロップするメソッド
        public Treasure DropTreasure()
        {
            // ドロップテーブルの定義（配列での初期化）
            var lootTable = new[]
            {
                new Treasure("金銀珊瑚", 50000, "眩く光る金銀と美しい珊瑚の山"),
                new Treasure("隠れ蓑", 15000, "着ると姿が見えなくなる不思議な蓑"),
                new Treasure("打ち出の小槌", 80000, "振れば欲しいものが出てくる魔法の小槌"),
                new Treasure("ただの石ころ", 1, "鬼ヶ島に転がっていた普通の石")
            };

            // Randomクラスを使って配列からランダムに1つ選ぶ
            var random = new Random();
            int index = random.Next(lootTable.Length);
            return lootTable[index];
        }
    }

    // --- 5. ジェネリクスクラス (Generics) ---
    public class Party<T> where T : Character
    {
        private readonly List<T> _members = new();

        public void AddMember(T member) => _members.Add(member);

        public IEnumerable<T> GetAliveMembers() => _members.Where(m => m.IsAlive);

        public bool IsWipedOut => !GetAliveMembers().Any();
    }

    // --- 6. メインプログラム ---
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("＝＝＝ 桃太郎（C# Edition） ＝＝＝\n");

            Console.WriteLine("むかしむかし、あるところに、おじいさんとおばあさんが住んでいました。");
            var grandpa = new Villager("おじいさん");
            var grandma = new Villager("おばあさん");

            grandpa.DoChore("山", "芝刈り");
            grandma.DoChore("川", "洗濯");
            await Task.Delay(1500);

            Console.WriteLine("\nおばあさんが川で洗濯をしていると...");
            Console.WriteLine("どんぶらこ、どんぶらこ...");
            await Task.Delay(1000);

            var giantPeach = new Peach("とても大きな");
            Console.WriteLine($"川上から {giantPeach.Size} 桃が流れてきました。");
            await Task.Delay(1500);

            Momotaro momotaro = giantPeach.CutOpen();
            Console.WriteLine($"二人はこの子を「{momotaro.Name}」と名付け、大切に育てました。\n");
            await Task.Delay(2000);

            Console.WriteLine($"立派に成長した {momotaro.Name} は、鬼ヶ島へ鬼退治に行く決意をしました。");

            momotaro.OnCompanionJoined += (name) =>
            {
                Console.WriteLine($"【システム】{name} がパーティーに加わりました！");
            };

            for (int i = 0; i < 3; i++)
            {
                momotaro.Obtain(new KibiDango(PowerBoost: 15));
            }

            Console.WriteLine("\n--- 仲間集めの旅 ---");
            var party = new Party<Character>();
            party.AddMember(momotaro);

            var animals = new List<Animal>
            {
                new Animal(AnimalType.Dog),
                new Animal(AnimalType.Monkey),
                new Animal(AnimalType.Pheasant)
            };

            foreach (var animal in animals)
            {
                Console.WriteLine($"\n{momotaro.Name}は {animal.Name} に出会った。");
                if (momotaro.GiveKibiDango(animal))
                {
                    party.AddMember(animal);
                }
                await Task.Delay(800);
            }

            Console.WriteLine("\n--- 鬼ヶ島へ出発 ---");
            Console.WriteLine("いざ、鬼ヶ島へ！ どんぶらこ、どんぶらこ...");
            await Task.Delay(2000);
            Console.WriteLine("鬼ヶ島に到着した！\n");

            var bossOni = new Oni("鬼の大将", hp: 200, attackPower: 40);
            Console.WriteLine("＝＝＝ バトル開始！ ＝＝＝");

            while (bossOni.IsAlive && !party.IsWipedOut)
            {
                foreach (var member in party.GetAliveMembers())
                {
                    if (!bossOni.IsAlive) break;

                    if (member is IAttacker attacker)
                    {
                        attacker.Attack(bossOni);
                        await Task.Delay(500);
                    }
                }

                if (!bossOni.IsAlive) break;

                Console.WriteLine("\n--- 鬼のターン ---");
                var aliveMembers = party.GetAliveMembers().ToList();
                var target = aliveMembers[new Random().Next(aliveMembers.Count)];
                bossOni.Attack(target);

                Console.WriteLine("------------------");
                await Task.Delay(1000);
            }

            Console.WriteLine("\n＝＝＝ 結果 ＝＝＝");
            if (!bossOni.IsAlive)
            {
                Console.WriteLine("桃太郎たちは見事、鬼を退治しました！\n");

                // 宝物のドロップ処理
                await Task.Delay(1000);
                Treasure droppedLoot = bossOni.DropTreasure();
                Console.WriteLine($"【ドロップ】鬼は『{droppedLoot.Name}』を落とした！");
                Console.WriteLine($"（詳細: {droppedLoot.Description} / 価値: {droppedLoot.Value}G）\n");

                momotaro.Treasures.Add(droppedLoot);

                Console.WriteLine("宝物を車に積んで、おじいさんとおばあさんの待つ村へ帰りました。");
                await Task.Delay(2000);

                Console.WriteLine("\n--- 村にて ---");
                grandpa.Rejoice(momotaro.Treasures[0]);
                grandma.Rejoice(momotaro.Treasures[0]);

                if (droppedLoot.Name == "ただの石ころ")
                {
                    Console.WriteLine("\nおじいさん・おばあさん「（……石ころかぁ……）」");
                }

                Console.WriteLine("\nめでたし、めでたし。");
            } else
            {
                Console.WriteLine("桃太郎たちは全滅してしまった...");
            }
        }
    }
}