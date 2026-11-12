package vrprof;

public final class TextSummaryFormatter implements SummaryFormatter {
    public String format(Exercise exercise, TrialAttempt attempt, TrialResult result) {
        var text = new StringBuilder();
        text.append("Проба: ").append(exercise.title()).append(".\n");
        text.append("Код школьника: ").append(attempt.pupilCode()).append(".\n");
        text.append("Выполнено шагов: ").append(result.completedSteps()).append(" из ").append(result.totalSteps()).append(".\n");
        text.append("Подсказок: ").append(result.hints()).append("; ошибок: ").append(result.errors()).append(".\n");
        text.append("Сложность: ").append(attempt.difficulty()).append("/5; интерес: ").append(attempt.interest()).append("/5.\n");
        text.append("Самооценка школьника: ").append(attempt.studentFeedback()).append("\n");
        text.append("Комментарий наставника: ").append(attempt.mentorFeedback());
        for (String detail : result.errorDetails()) text.append("\n").append(detail);
        return text.toString().stripTrailing();
    }
}
