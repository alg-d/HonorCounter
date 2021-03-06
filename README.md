# HonorCounter
スマホアプリ「キングスレイド」におけるオナーリーグの勝敗数を自動でカウントするツールです。
OBS等で配信画面に載せて使います。

実際に使用してみた配信→https://www.youtube.com/watch?v=bBGDZMKiHRM

## 動作環境

* Windows 10
* .NET 6

## 使い方
![ツールの画面イメージ](http://alg-d.com/HonorCounter.png)
1. 「①対象ウィンドウ」のプルダウンにウィンドウ一覧が表示されるので「キングスレイド」が表示されているウィンドウを選択します。
    * このウィンドウ一覧はプログラム実行開始時のものが表示されるので、更新したい場合は「一覧更新」ボタンをクリックします。
2. 「②確認」ボタンをクリックすると、選択したウィンドウの画面イメージが表示されるので「キングスレイド」のゲーム画面が表示されていることを確認してください。
3. 「③スタート」ボタンをクリックすると、勝敗数カウント処理が始まります。
    * 処理中は1秒毎に対象ウィンドウを確認し、「VICTORY」が表示されていれば勝ち、「LOSE」が表示されていれば負け、と判断します。
    * カウントを元に「③スタート」ボタンの下にテキストを表示します。
    * テキストのフォントや色は自由に指定できます。
    * テキストは「フォーマット」欄の形式で表示されます。その際、次の変数を使用することができます。
        * {0}: 勝ち数
        * {1}: 負け数
        * {2}: 試合数
        * {3}: 連勝数
    * 下の「+」「-」ボタンでカウントを手動で変更することができます。
    * 「リセット」ボタンをクリックするとカウンターを全て0に戻します。
    * 「コピー」ボタンをクリックするとテキストの内容をクリップボードにコピーします。
4. 「④ストップ」ボタンをクリックすると、処理を停止します。

## 注意
自由に使ってもらって構いませんが、他の環境で勝敗の判定がうまく動くかは分かりません。
使いたいけど動かない場合は [@alg_d](https://twitter.com/alg_d) までご連絡ください。
