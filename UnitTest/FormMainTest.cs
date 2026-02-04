using Omron_Emu;

namespace UnitTest
{
    public class FormMainTest
    {
        [Fact]
        public void TestAdd()
        {
            using(FormMain form = new FormMain())
            {
                Assert.Equal(5, form.Add(3, 2));
            }
        }

        [Fact]
        public void TestSubtract()
        {
            using(FormMain form = new FormMain())
            {
                Assert.Equal(1, form.Subtract(3, 2));
            }
        }

        [Fact]
        public void TestRELU()
        {
            using (FormMain form = new FormMain())
            {
                Assert.Equal(3.14, form.RELU(3.14));
                Assert.Equal(0.0, form.RELU(-2.71));
            }
        }

        [Fact]
        public void TestSquare()
        {
            using (FormMain form = new FormMain())
            {
                Assert.Equal(9, form.Square(3));
            }
        }
    }
}