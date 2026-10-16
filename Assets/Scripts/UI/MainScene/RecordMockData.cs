public static class RecordMockData
{
    // 최근 경기부터 정렬한 UI 확인용 데이터입니다. 실제 저장 파일은 사용하지 않습니다.
    public static RecordListItem[] Create()
    {
        return new[]
        {
            CreateRecord(2, 5, "TEST TEAM", 1, 2),
            CreateRecord(2, 4, "BLUE TEAM", 2, 1, 11),
            CreateRecord(2, 3, "RED TEAM", 2, 0, 10),
            CreateRecord(2, 2, "GREEN TEAM", 0, 2, 1),
            CreateRecord(2, 1, "WHITE TEAM", 2, 1, 0),
            CreateRecord(2, 12, "BLACK TEAM", 1, 2),
            CreateRecord(2, 11, "TEST TEAM", 2, 0),
            CreateRecord(1, 5, "BLUE TEAM", 0, 2),
            CreateRecord(1, 4, "RED TEAM", 2, 1),
            CreateRecord(1, 3, "GREEN TEAM", 1, 2),
            CreateRecord(1, 2, "WHITE TEAM", 2, 0),
            CreateRecord(1, 1, "BLACK TEAM", 0, 2)
        };
    }

    private static RecordListItem CreateRecord(int year, int month, string opponent, int playerWins, int opponentWins, int lineCount = 25)
    {
        return new RecordListItem
        {
            CareerYear = year,
            Month = month,
            OpponentTeamName = opponent,
            PlayerSetWins = playerWins,
            OpponentSetWins = opponentWins,
            CommentaryLines = CreateCommentary(opponent, playerWins, opponentWins, lineCount)
        };
    }

    private static string[] CreateCommentary(string opponent, int playerWins, int opponentWins, int lineCount)
    {
        string winner = playerWins > opponentWins ? "우리 팀" : opponent;
        string[] commentary =
        {
            $"우리 팀과 {opponent}의 경기가 시작됩니다.",
            "우리 팀이 첫 서브를 준비합니다.",
            "서브가 상대 코트 깊숙이 향합니다.",
            $"{opponent}이 리시브로 공을 띄웁니다.",
            "상대 세터가 공격수 앞으로 토스합니다.",
            "상대 공격수가 스파이크를 시도합니다.",
            "우리 팀이 블로킹으로 막아냅니다.",
            "우리 팀이 첫 득점을 가져갑니다.",
            "우리 팀의 다음 서브가 네트를 넘어갑니다.",
            $"{opponent}이 안정적으로 받아냅니다.",
            "상대 공격이 블로킹 옆을 통과합니다.",
            "상대 팀이 한 점을 따라붙습니다.",
            "이번에는 상대 팀의 서브입니다.",
            "우리 팀이 낮은 자세로 리시브합니다.",
            "우리 팀 세터가 빠르게 공을 연결합니다.",
            "우리 팀 공격수가 높이 뛰어오릅니다.",
            "스파이크가 상대 코트 빈 곳에 떨어집니다.",
            "우리 팀이 다시 한 점을 추가합니다.",
            "양 팀이 긴 랠리를 이어갑니다.",
            "우리 팀이 어려운 공을 살려냅니다.",
            $"{opponent}도 수비에 성공합니다.",
            "끝까지 집중하는 양 팀 선수들입니다.",
            "마지막 득점을 위한 공격이 이어집니다.",
            $"{winner}이 마지막 세트를 가져갑니다.",
            $"경기 종료! 최종 세트 스코어 {playerWins}:{opponentWins}, {winner}의 승리입니다."
        };

        string[] lines = new string[lineCount];
        System.Array.Copy(commentary, lines, lineCount);
        return lines;
    }
}
