using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace ACT4
{
    public partial class Form1 : Form
    {
        int side;
        int n = 6; 
        int beamWidth = 3; 
        int moveCounter;
        ArrayList currentStates; 

        public Form1()
        {
            InitializeComponent();
            side = pictureBox1.Width / n;
            initializeBeam(); 
            updateUI();
        }

        private void initializeBeam()
        {
            currentStates = new ArrayList();

            for (int i = 0; i < beamWidth; i++)
            {
                currentStates.Add(randomSixState());
            }

            moveCounter = 0;
            updateUI();
        }

        // Get the number of attacking pairs 
        private int getAttackingPairs(SixState f)
        {
            int attackers = 0;

            for (int rf = 0; rf < n; rf++)
            {
                for (int tar = rf + 1; tar < n; tar++)
                {
                    if (f.Y[rf] == f.Y[tar] || Math.Abs(f.Y[rf] - f.Y[tar]) == tar - rf)
                        attackers++;
                }
            }

            return attackers;
        }
        
        private int[,] getHeuristicTableForPossibleMoves(SixState thisState)
        {
            int[,] hStates = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    SixState possible = new SixState(thisState);
                    possible.Y[i] = j;
                    hStates[i, j] = getAttackingPairs(possible);
                }
            }

            return hStates;
        }

        // Get the best moves for a given heuristic table
        private ArrayList getBestMoves(int[,] heuristicTable, SixState thisState)
        {
            ArrayList bestMoves = new ArrayList();
            int bestHeuristicValue = heuristicTable[0, 0];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (bestHeuristicValue > heuristicTable[i, j])
                    {
                        bestHeuristicValue = heuristicTable[i, j];
                        bestMoves.Clear();
                        if (thisState.Y[i] != j)
                            bestMoves.Add(new Point(i, j));
                    }
                    else if (bestHeuristicValue == heuristicTable[i, j])
                    {
                        if (thisState.Y[i] != j)
                            bestMoves.Add(new Point(i, j));
                    }
                }
            }

            return bestMoves;
        }

        // Execute the local beam search
        private void executeLocalBeamSearch()
        {
            Random rand = new Random();

            while (true)
            {
                ArrayList nextStates = new ArrayList();

                foreach (SixState state in currentStates)
                {
                    int[,] hTable = getHeuristicTableForPossibleMoves(state);
                    ArrayList bestMoves = getBestMoves(hTable, state);

                    foreach (Point move in bestMoves)
                    {
                        SixState newState = new SixState(state);
                        newState.Y[move.X] = move.Y;
                        nextStates.Add(newState);
                    }
                }

                nextStates.Sort((x, y) => getAttackingPairs((SixState)x).CompareTo(getAttackingPairs((SixState)y)));

                currentStates.Clear();
                for (int i = 0; i < Math.Min(beamWidth, nextStates.Count); i++)
                {
                    currentStates.Add(nextStates[i]);

                    if (getAttackingPairs((SixState)nextStates[i]) == 0)
                    {
                        currentStates.Clear(); 
                        currentStates.Add(nextStates[i]);
                        updateUI();
                        return; 
                    }
                }

                moveCounter++;
                updateUI();
            }
        }

        // Update the UI to show the current state and statistics
        private void updateUI()
        {
            pictureBox1.Refresh();
            pictureBox2.Refresh();

            label4.Text = "Moves: " + moveCounter;

            if (currentStates.Count > 0)
            {
                label3.Text = "Attacking pairs (Best State): " + getAttackingPairs((SixState)currentStates[0]);
            }
        }

        // Paint the current best state on the board
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (currentStates.Count == 0) return;

            SixState bestState = (SixState)currentStates[0];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, i * side, j * side, side, side);
                    }
                    if (j == bestState.Y[i])
                    {
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                    }
                }
            }
        }

        // Button to start the Local Beam Search
        private void button1_Click(object sender, EventArgs e)
        {
            executeLocalBeamSearch();
        }

        // Generate a random N-Queens state
        private SixState randomSixState()
        {
            Random rand = new Random();
            return new SixState(rand.Next(n), rand.Next(n), rand.Next(n), rand.Next(n), rand.Next(n), rand.Next(n));
        }
    }

    // Class to represent the state of the N-Queens board
    public class SixState
    {
        public int[] Y; 

        public SixState(params int[] queens)
        {
            Y = queens;
        }

        public SixState(SixState other)
        {
            Y = (int[])other.Y.Clone();
        }
    }
}
