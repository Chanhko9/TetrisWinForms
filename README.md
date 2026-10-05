# Tetris WinForms C# — Final Candidate

Đồ án game Tetris WinForms C# với 5 chế độ chơi, nhạc nền riêng, hiệu ứng hình ảnh, chỉnh âm lượng, leaderboard và lưu lịch sử bằng TXT.

## Yêu cầu môi trường
- Visual Studio 2022 có workload **.NET desktop development**.
- .NET 8 SDK.
- NuGet sẽ restore `NAudio 2.4.0` khi build.

## Chạy project
1. Mở `TetrisWinForms.sln`.
2. Chờ Visual Studio restore NuGet.
3. Build Solution (`Ctrl + Shift + B`).
4. Run (`F5`).

## 5 chế độ
1. **Basic** — Tetris truyền thống.
2. **Arcane Chaos** — 9 relic ma thuật phá board, nhấn `F` để dùng.
3. **Phantom Fog** — fog che vùng dưới; clear line để nhìn lại tạm thời.
4. **Zero-G Rift** — gravity thay đổi `↓ → ↑ ←`; hàng/cột clear theo hướng gravity.
5. **Cursed Tide** — garbage row dâng từ đáy theo chu kỳ.

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

## Portable — không dùng absolute path
Project KHÔNG dùng các path kiểu `C:\\Users\\...` hoặc `D:\\...`.

Assets và data đều được tìm từ:
```csharp
AppContext.BaseDirectory
```

Ví dụ:
```csharp
Path.Combine(AppContext.BaseDirectory, "Assets", "Audio", "basic_theme.mp3")
```

## Publish để đem sang máy khác
Project có profile:
`Properties/PublishProfiles/WinX64SelfContained.pubxml`

Trong Visual Studio:
1. Right click project `TetrisWinForms`.
2. Chọn **Publish**.
3. Chọn profile `WinX64SelfContained`.
4. Publish.
5. Copy **toàn bộ folder publish** sang máy khác.

Profile dùng `SelfContained=true`, `win-x64`, nên bản publish mang runtime .NET theo cùng ứng dụng. Không copy riêng file `.exe`; phải copy nguyên folder publish vì game còn có `Assets` và `Data`.

## Dữ liệu điểm
Mỗi record TXT có dạng:
```text
PlayerName|Score|Mode|yyyy-MM-dd HH:mm:ss
```

Ví dụ:
```text
Huy|12500|ArcaneChaos|2026-09-30 16:40:00
```

## Lưu ý trước khi nộp
Chạy `FINAL_TEST_CHECKLIST.md` ít nhất một lần trên máy phát triển và, nếu có thể, trên một máy Windows khác bằng folder Publish.


## Architecture
UI and gameplay logic are separated. See `ARCHITECTURE.md`.

## Scoring (current build)
- Each cleared row: **+1 point**.
- Combo bonus: `combo streak - 1` when consecutive piece locks clear at least one row.
- Back-to-Back: **+2 bonus** when a 4-line clear follows another 4-line clear without a 1–3 line clear breaking the chain.
- Soft Drop / Hard Drop / movement / rotation do not award points.

Team: `Sunrise-MSang_VMinh_PNam_MQuân`
