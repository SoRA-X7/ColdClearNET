using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static ColdClearNET.Interface;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace ColdClearNET {
    /// <summary>
    /// Cold Clear Bot main class
    /// </summary>
    public class ColdClear : IDisposable {
        private readonly IntPtr ptr;

        /// <summary>
        /// Launches a bot thread with a blank board, empty queue, and all seven pieces in the bag, using the specified options and weights.
        /// DO NOT forget to call Dispose() when you are done with the bot instance.
        /// </summary>
        /// <param name="options">Options that will be passed to the bot.</param>
        /// <param name="weights">Evaluation weights that will be passed to the bot.</param>
        public ColdClear(in CCOptions options, in CCWeights weights) {
            ptr = cc_launch_async(options, weights);
        }

        /// <summary>
        /// Launches a bot thread with a blank board, empty queue, and all seven pieces in the bag, using the default options and weights.
        /// DO NOT forget to call Dispose() when you are done with the bot instance.
        /// </summary>
        public ColdClear() : this(GetDefaultOptions(), GetDefaultWeights()) { }

        /// <summary>
        /// Adds a new piece to the end of the queue.
        ///
        /// If speculation is enabled, the piece must be in the bag.
        /// For example, if you start a new game with starting sequence IJOZT, the first time you call this function you can only provide either an L or an S piece.
        /// </summary>
        /// <param name="piece">Piece to add.</param>
        public void AddPiece(CCPiece piece) {
            cc_add_next_piece_async(ptr, piece);
        }

        /// <summary>
        /// Adds new pieces to the end of the queue.
        /// </summary>
        /// <seealso cref="AddPiece"/>
        /// <param name="pieces">Enumerable of pieces to append.</param>
        public void AddPieces(IEnumerable<CCPiece> pieces) {
            foreach (var piece in pieces) {
                cc_add_next_piece_async(ptr, piece);
            }
        }

        /// <summary>
        /// Resets the playfield, back-to-back status, and combo count.
        ///
        /// This should only be used when garbage is received or when your client could not place the piece in the correct position for some reason (e.g. 15 move rule), since this forces the bot to throw away previous computations.
        ///
        /// Note: combo is not the same as the displayed combo in guideline games. Here, it is the number of consecutive line clears achieved. So, generally speaking, if "x Combo" appears on the screen, you need to use x+1 here.
        /// </summary>
        /// <param name="field">an array of 400 booleans in row major order, with index 0 being the bottom-left cell.</param>
        /// <param name="b2b">back-to-back status</param>
        /// <param name="combo">combo value</param>
        public void Reset(bool[] field, bool b2b, uint combo) {
            cc_reset_async(ptr, field, b2b, combo);
        }

        /// <summary>
        /// Requests the bot to provide a move as soon as possible.
        /// 
        /// In most cases, "as soon as possible" is a very short amount of time, and is only longer if the provided lower limit on thinking has not been reached yet or if the bot cannot provide a move yet, usually because it lacks information on the next pieces.
        /// 
        /// For example, in a game with zero piece previews and hold enabled, the bot will never be able to provide the first move because it cannot know what piece it will be placing if it chooses to hold. Another example: in a game with zero piece previews and hold disabled, the bot will only be able to provide a move after the current piece spawns and you provide the piece information to the bot using <c>AddPiece()</c> or <c>AddPieces()</c>.
        /// 
        /// It is recommended that you call this function the frame before the piece spawns so that the bot has time to finish its current thinking cycle and supply the move.
        /// 
        /// Once a move is chosen, the bot will update its internal state to the result of the piece being placed correctly and the move will become available by calling <c>PollNextMove()</c>.
        /// </summary>
        /// <param name="incoming">the number of lines of garbage the bot is expected to receive after placing the next piece.</param>
        public void RequestNextMove(uint incoming) {
            cc_request_next_move(ptr, incoming);
        }

        /// <summary>
        /// Checks to see if the bot has provided the previously requested move yet.
        /// 
        /// The returned move contains both a path and the expected location of the placed piece.
        /// The returned path is reasonably good, but you might want to use your own pathfinder to, for example, exploit movement intricacies in the game you're playing.
        /// 
        /// If the piece couldn't be placed in the expected location, you must call <c>Reset()</c> to reset the game field, back-to-back status, and combo values.
        /// 
        /// </summary>
        /// <param name="move">If the move has been provided, this function will return true and the move will be returned in the move parameter. Otherwise, this function returns false.</param>
        /// <returns>If the move has been provided, this function will return true and the move will be returned in the move parameter. Otherwise, this function returns false.</returns>
        public bool PollNextMove(out CCMove move) {
            return cc_poll_next_move(ptr, out move);
        }

        /// <summary>
        /// Requests the bot to provide a move as soon as possible and returns it.
        ///
        /// In most cases, "as soon as possible" is a very short amount of time, and is only longer if the provided lower limit on thinking has not been reached yet or if the bot cannot provide a move yet, usually because it lacks information on the next pieces.
        /// 
        /// For example, in a game with zero piece previews and hold enabled, the bot will never be able to provide the first move because it cannot know what piece it will be placing if it chooses to hold. Another example: in a game with zero piece previews and hold disabled, the bot will only be able to provide a move after the current piece spawns and you provide the piece information to the bot using <c>AddPiece()</c> or <c>AddPieces()</c>.
        /// </summary>
        /// <param name="incoming">the number of lines of garbage the bot is expected to receive after placing the next piece.</param>
        /// <param name="pollInterval">interval of checking whether the next move is ready or not.</param>
        /// <returns>the provided move.</returns>
        public async Task<CCMove> GetNextMoveAsync(uint incoming, uint pollInterval = 25) {
            cc_request_next_move(ptr, incoming);
            while (true) {
                if (cc_poll_next_move(ptr, out var move)) return move;
                await Task.Delay((int) pollInterval);
            }
        }

        /// <summary>
        /// Returns true if all possible piece placement sequences result in death, or the bot thread crashed.
        /// </summary>
        public bool IsDead => cc_is_dead_async(ptr);

        private void ReleaseUnmanagedResources() {
            cc_destroy_async(ptr);
        }

        /// <summary>
        /// Terminates the bot thread and frees the memory associated with the bot.
        /// </summary>
        public void Dispose() {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~ColdClear() {
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Returns the default options
        /// </summary>
        /// <returns>default options</returns>
        public static CCOptions GetDefaultOptions() {
            cc_default_options(out var options);
            return options;
        }

        /// <summary>
        /// Returns the default weights
        /// </summary>
        /// <returns>default weights</returns>
        public static CCWeights GetDefaultWeights() {
            cc_default_weights(out var weights);
            return weights;
        }

        /// <summary>
        /// Returns the fast game config weights
        /// </summary>
        /// <returns>fast game config weights</returns>
        public static CCWeights GetFastWeights() {
            cc_fast_weights(out var weights);
            return weights;
        }
    }

    public enum CCPiece : uint {
        CC_I,
        CC_T,
        CC_O,
        CC_S,
        CC_Z,
        CC_L,
        CC_J
    }

    public enum CCMovement : uint {
        CC_LEFT,
        CC_RIGHT,
        CC_CW,
        CC_CCW,

        /// <summary>
        /// soft drop all the way down
        /// </summary>
        CC_DROP
    }

    public enum CCMovementMode : uint {
        CC_0G,
        CC_20G,
        CC_HARD_DROP_ONLY
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CCMove {
        /// <summary>
        /// Whether hold is required
        /// </summary>
        [MarshalAs(UnmanagedType.U1)] public bool hold;

        /// <summary>
        /// Expected cell coordinates of placement, (0, 0) being the bottom left
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] expected_x;

        /// <summary>
        /// Expected cell coordinates of placement, (0, 0) being the bottom left
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] expected_y;

        /// <summary>
        /// Number of moves in the path
        /// </summary>
        public byte movement_count;

        /// <summary>
        /// Movements
        /// Length is always 32 so use <c>movement_count</c> as true length
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public CCMovement[] movements;

        public uint nodes;
        public uint depth;
        public uint original_rank;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CCOptions {
        public CCMovementMode mode;
        [MarshalAs(UnmanagedType.U1)] public bool use_hold;
        [MarshalAs(UnmanagedType.U1)] public bool speculate;
        public uint min_nodes;
        public uint max_nodes;
        public uint threads;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CCWeights {
        public int back_to_back;
        public int bumpiness;
        public int bumpiness_sq;
        public int height;
        public int top_half;
        public int top_quarter;
        public int jeopardy;
        public int cavity_cells;
        public int cavity_cells_sq;
        public int overhang_cells;
        public int overhang_cells_sq;
        public int covered_cells;
        public int covered_cells_sq;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] tslot;

        public int well_depth;
        public int max_well_depth;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public int[] well_column;

        public int b2b_clear;
        public int clear1;
        public int clear2;
        public int clear3;
        public int clear4;
        public int tspin1;
        public int tspin2;
        public int tspin3;
        public int mini_tspin1;
        public int mini_tspin2;
        public int perfect_clear;
        public int combo_garbage;
        public int move_time;
        public int wasted_t;
        [MarshalAs(UnmanagedType.U1)] public bool use_bag;
    }
}