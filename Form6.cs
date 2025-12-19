using System;
using System.Windows.Forms;

namespace NMHwin
{
    public partial class Form6 : Form
    {
        private Deck deck;  // Bộ bài

        public Form6()
        {
            InitializeComponent();  // Khởi tạo form
            deck = new Deck();      // Khởi tạo bộ bài
            this.ClientSize = new Size(1200, 800);

        }

        // Sự kiện khi form được tải
        private void Form6_Load(object sender, EventArgs e)
        {
            // Nếu bạn muốn thêm các thao tác khởi tạo khác khi form được tải, có thể thêm vào đây.
        }

        // Sự kiện khi người chơi nhấn nút "Draw Cards"
        private void BtnDraw_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra nếu bộ bài còn lá
                if (!deck.HasCards())
                {
                    MessageBox.Show("The deck is empty! Resetting the deck.", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    deck.ResetDeck();  // Reset lại bộ bài
                }

                // Rút 3 lá bài cho người chơi và máy tính
                int playerTotal = 0;
                int computerTotal = 0;

                string playerCards = "";
                string computerCards = "";

                for (int i = 0; i < 3; i++)
                {
                    // Rút bài cho người chơi và máy tính
                    Card playerCard = deck.DrawCard();
                    Card computerCard = deck.DrawCard();

                    // Thêm từng lá bài vào chuỗi
                    playerCards += playerCard.ToString() + "\n";
                    computerCards += computerCard.ToString() + "\n";

                    // Cộng dồn điểm cho người chơi và máy tính
                    playerTotal += playerCard.Value;
                    computerTotal += computerCard.Value;
                }

                // Hiển thị các lá bài của người chơi và máy tính
                lblPlayerCard.Text = "Your cards:\n" + playerCards + "\nTotal: " + playerTotal;
                lblComputerCard.Text = "Computer's cards:\n" + computerCards + "\nTotal: " + computerTotal;

                // Kiểm tra nếu tổng điểm vượt quá 21
                if (playerTotal > 21)
                {
                    lblResult.Text = "You lose!";  // Người chơi thua nếu tổng điểm vượt quá 21
                    return;  // Dừng lại khi người chơi thua
                }

                if (computerTotal > 21)
                {
                    lblResult.Text = "Computer loses!";  // Máy tính thua nếu tổng điểm vượt quá 21
                    return;  // Dừng lại khi máy tính thua
                }

                // So sánh tổng điểm
                string result;
                if (playerTotal > computerTotal)
                {
                    result = "You win!";
                }
                else if (playerTotal < computerTotal)
                {
                    result = "Computer wins!";
                }
                else
                {
                    result = "It's a tie!";
                }

                lblResult.Text = result;  // Hiển thị kết quả
            }
            catch (InvalidOperationException ex)
            {
                // Nếu bộ bài hết, thông báo cho người chơi
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





        // Lớp Card đại diện cho 1 lá bài
        public class Card
        {
            public string Suit { get; set; }  // Chất (♥, ♠, ♦, ♣)
            public string Rank { get; set; }  // Hạng (2, 3, 4, ..., K, A)
            public int Value { get; set; }    // Giá trị của lá bài (2-10, J=11, Q=12, K=13, A=14)

            public Card(string suit, string rank, int value)
            {
                Suit = suit;
                Rank = rank;
                Value = value;
            }

            // Hàm để tính lại giá trị của A khi tổng điểm lớn hơn 21
            public int GetAdjustedValue(int currentTotal)
            {
                if (Rank == "A" && currentTotal + 11 > 21)
                {
                    return 1; // Nếu tổng điểm lớn hơn 21, giá trị của A sẽ là 1
                }
                return Value; // Nếu không, giá trị của A là 11 hoặc 1 tuỳ theo tình huống
            }

            public override string ToString()
            {
                return $"{Rank} of {Suit}";
            }
        }


        // Lớp Deck đại diện cho bộ bài
        public class Deck
        {
            private List<Card> cards = new List<Card>();
            private Random rand = new Random();

            public Deck()
            {
                // Tạo bộ bài với 52 lá
                string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
                string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
                int[] values = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 1 };

                foreach (var suit in suits)
                {
                    for (int i = 0; i < ranks.Length; i++)
                    {
                        cards.Add(new Card(suit, ranks[i], values[i]));
                    }
                }
            }

            // Rút bài
            public Card DrawCard()
            {
                if (cards.Count == 0)  // Kiểm tra nếu bộ bài đã hết
                {
                    throw new InvalidOperationException("The deck is empty! Cannot draw any more cards.");
                }

                int index = rand.Next(cards.Count);  // Lấy ngẫu nhiên 1 lá bài
                Card card = cards[index];
                cards.RemoveAt(index);  // Loại bỏ lá bài khỏi bộ
                return card;
            }

            // Kiểm tra nếu bộ bài còn lá bài
            public bool HasCards()
            {
                return cards.Count > 0;
            }

            // Reset lại bộ bài
            public void ResetDeck()
            {
                cards.Clear();
                string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
                string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
                int[] values = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };

                foreach (var suit in suits)
                {
                    for (int i = 0; i < ranks.Length; i++)
                    {
                        cards.Add(new Card(suit, ranks[i], values[i]));
                    }
                }
            }
        }

    }
}
