using System;
using System.Runtime.InteropServices;

namespace ColdClearNET {
    internal static class Interface {
        [DllImport("cold_clear.dll")]
        public static extern IntPtr cc_launch_async(CCOptions options, CCWeights weights);

        [DllImport("cold_clear.dll")]
        public static extern void cc_destroy_async(IntPtr bot);

        [DllImport("cold_clear.dll")]
        public static extern void cc_reset_async(IntPtr bot,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)]
            bool[] field,
            [MarshalAs(UnmanagedType.U1)] bool b2b,
            uint combo);

        [DllImport("cold_clear.dll")]
        public static extern void cc_add_next_piece_async(IntPtr bot, CCPiece piece);

        [DllImport("cold_clear.dll")]
        public static extern void cc_request_next_move(IntPtr bot, uint incoming);

        [DllImport("cold_clear.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool cc_poll_next_move(IntPtr bot, out CCMove move);

        [DllImport("cold_clear.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool cc_is_dead_async(IntPtr bot);

        [DllImport("cold_clear.dll")]
        public static extern void cc_default_options(out CCOptions options);

        [DllImport("cold_clear.dll")]
        public static extern void cc_default_weights(out CCWeights weights);

        [DllImport("cold_clear.dll")]
        public static extern void cc_fast_weights(out CCWeights weights);
    }
}