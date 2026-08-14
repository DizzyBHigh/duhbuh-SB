// This file documents the method surface expected by the Lurks runtime.
// Streamer.bot Execute C# Method subactions only expose public parameterless bool methods.
// The implementation in Lurks.cs should expose these wrappers and keep Execute()
// for backwards-compatible argument dispatch.
//
// Expected public methods:
//   bool StartLurk()
//   bool EndLurk()
//   bool CheckLurkers()
//   bool Stats()
//   bool Leaderboard()
//   bool ChatUnlurk()
//   bool RemoveUnpresentLurkers()
