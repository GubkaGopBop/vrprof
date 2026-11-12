package vrprof;

import java.util.List;

public record TrialAttempt(String attemptId, String exerciseId, String pupilCode, String mentorCode,
                           List<TrialEvent> events, int difficulty, int interest,
                           String studentFeedback, String mentorFeedback) {
    public TrialAttempt {
        if (attemptId == null || attemptId.isBlank() || exerciseId == null || exerciseId.isBlank()
                || pupilCode == null || pupilCode.isBlank() || mentorCode == null || mentorCode.isBlank())
            throw new IllegalArgumentException("Идентификаторы попытки, задания и участников обязательны.");
        if (events == null || studentFeedback == null || mentorFeedback == null)
            throw new IllegalArgumentException("Журнал и отзывы не могут быть null.");
        if (difficulty < 1 || difficulty > 5 || interest < 1 || interest > 5)
            throw new IllegalArgumentException("Оценки должны быть от 1 до 5.");
        events = List.copyOf(events);
    }
}
