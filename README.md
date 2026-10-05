# Tetris WinForms C# — Final Candidate

Đồ án game Tetris WinForms C# với 6 chế độ chơi, nhạc nền riêng, hiệu ứng hình ảnh, chỉnh âm lượng, leaderboard và lưu lịch sử bằng TXT.

## Yêu cầu môi trường
- Visual Studio 2022 có workload **.NET desktop development**.
- .NET 8 SDK.
- NuGet restore `NAudio 2.4.0` khi build.

## Chạy project
1. Mở `TetrisWinForms.sln`.
2. Chờ Visual Studio restore NuGet.
3. Build Solution (`Ctrl + Shift + B`).
4. Run (`F5`).

## 6 chế độ
1. **Basic** — Tetris truyền thống.
2. **Arcane Chaos** — 9 relic ma thuật phá board, nhấn `F` để dùng.
3. **Phantom Fog** — fog che vùng dưới; clear line để nhìn lại tạm thời.
4. **Zero-G Rift** — gravity thay đổi `↓ → ↑ ←`; hàng/cột clear theo hướng gravity.
5. **Cursed Tide** — garbage row dâng từ đáy theo chu kỳ.
6. **Dual Area** - đấu đôi.

## Điều khiển
- `A / D` hoặc `← / →`: di chuyển ngang theo gravity.
- `W / ↑`: xoay.
- `S / ↓`: soft drop.
- `SPACE`: hard drop.
- `C`: HOLD.
- `P / ESC`: pause/resume.
- `F`: dùng relic trong Arcane Chaos.

## Các chức năng nổi bật
- 7-bag Tetromino random.
- NEXT + HOLD + ghost piece.
- Combo + Back-to-Back scoring.
- Start countdown `3-2-1-GO`.
- Pause overlay.
- Line clear animation + hard-drop impact.
- Screen shake / particles / warning banner.
- 9 Arcane relic: Rune Bomb, Dragon Breath, Thunder Spear, Void Cross, Prism Relic, Meteor Relic, Black Hole, Phoenix Sigil, Chaos Dice.
- Nhạc riêng cho menu và từng mode.
- SFX cho gameplay / mode events / relic.
- Master volume + mute.
- Game Over hiện điểm ngay rồi mở Top 10 của mode.
- Record hiện tại được highlight; có `NEW MODE BEST` / mode rank.
- History đọc từ `Data/score_history.txt`.
- How To Play ngay trong Main Menu.
