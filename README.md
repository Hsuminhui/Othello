# 黑白棋 遊戲

本專案使用 C# Windows Forms 製作的黑白棋遊戲。  
遊戲以 8 × 8 棋盤為基礎，實作黑白棋的基本規則，包含合法落子判斷、棋子翻轉、分數統計、悔棋、重新開始，以及不同難易度的 AI 對戰功能。

---

## 一、專案簡介

本專案是一款黑白棋遊戲，玩家可以透過滑鼠點擊棋盤進行落子。  

系統會自動判斷該位置是否為合法落點，若符合規則，會自動翻轉被夾住的對手棋子。

遊戲中黑棋先手，白棋後手。  

當某一方沒有合法落點時，系統會自動跳過該回合(如下圖所示)；若雙方都沒有合法落點，則遊戲結束並計算勝負。

系統介面如下:

<img width="718" height="723" alt="image" src="https://github.com/user-attachments/assets/a2af2934-1cff-4dcd-83c7-e6ff39aaab66" />

系統提醒跳過回合:

<img width="190" height="119" alt="image" src="https://github.com/user-attachments/assets/8ddf66f0-1795-490b-8a63-1d5c88ff56f8" />

遊戲結束，勝負宣告:

<img width="118" height="134" alt="image" src="https://github.com/user-attachments/assets/1b7b76b8-467e-4068-b53a-ed966f40082d" />


---

## 二、主要功能

### 1. 黑白棋基本遊戲規則

- 使用 8 × 8 棋盤
- 黑棋先手
- 玩家必須在可以夾住對方棋子的地方落子
- 被夾住的對手棋子會自動翻轉成自己的顏色
- 系統會自動判斷合法落點
- 雙方都無法落子時，遊戲結束

<img width="277" height="271" alt="image" src="https://github.com/user-attachments/assets/e0c5a39c-f09e-44d8-82da-41fefb881111" />

---

### 2. 棋盤繪製

遊戲棋盤使用 `Panel` 元件繪製，包含：

- 綠色棋盤背景
- 8 × 8 格線
- 黑棋與白棋棋子
- 棋子陰影與高光效果
- 可落子位置提示

---

### 3. 難度模式選擇

可調整遊戲進行的模式，本專案預設為**人 vs 人**

- 人 vs 人
- 人 vs AI (難度: 簡易)
- 人 vs AI (難度: 稍難)

<img width="130" height="128" alt="image" src="https://github.com/user-attachments/assets/533a419f-d6ff-4085-bc56-2618e4d1b6dd" />

---

### 4. 合法落子提示

當輪到玩家時，棋盤上會顯示半透明小圓點，提示目前可以落子的位置。  
玩家只能點擊合法位置進行落子。

系統提示目前可落子的位置:

<img width="245" height="245" alt="image" src="https://github.com/user-attachments/assets/3710d365-470b-47d8-93ed-342dbc56b711" />

---

### 5. 悔棋按鈕

當玩家誤觸or想要更動上一步落子位置，本專案提供悔棋功能，能返回上一步，如下圖所示:  

落子前:

<img width="358" height="361" alt="image" src="https://github.com/user-attachments/assets/20ba6acc-ef3d-4223-a926-1e6761a6c549" />


落子後:

<img width="359" height="358" alt="image" src="https://github.com/user-attachments/assets/7257972c-8bbe-4180-85bb-5cfb2632800d" />


點擊**悔棋按鈕**後:

<img width="358" height="360" alt="image" src="https://github.com/user-attachments/assets/4348cd7f-45ed-497e-901d-2e1a0a0e625b" />


---

### 6. 棋子翻轉

玩家落子後，程式會檢查八個方向：

- 上
- 下
- 左
- 右
- 左上
- 右上
- 左下
- 右下

只要某個方向可以夾住對手棋子，該方向中間的對手棋子就會全部翻轉。

---

### 7. 分數統計

遊戲畫面會即時顯示：

- 黑棋目前數量
- 白棋目前數量
- 目前輪到哪一方

---
