package vrprof;

import java.util.ArrayList;
import java.util.Objects;

public final class AttemptEvaluator {
    public TrialResult evaluate(Exercise exercise, TrialAttempt attempt) {
        Objects.requireNonNull(exercise, "exercise");
        Objects.requireNonNull(attempt, "attempt");
        if (!exercise.id().equals(attempt.exerciseId()))
            throw new IllegalArgumentException("Неизвестная версия задания: " + attempt.exerciseId());
        int completed = 0, hints = 0, expectedNumber = 1;
        var details = new ArrayList<String>();
        for (TrialEvent event : attempt.events()) {
            if (event.number() != expectedNumber++)
                throw new IllegalArgumentException("Номера событий должны идти подряд, начиная с 1.");
            if (event.kind().equals("hint")) { hints++; continue; }
            boolean matches = completed < exercise.steps().size()
                && event.stepCode().equals(exercise.steps().get(completed).code())
                && event.objectId().equals(exercise.steps().get(completed).objectId());
            if (matches && event.success()) completed++;
            else details.add("Событие " + event.number() + ": действие " + event.stepCode()
                + " с объектом " + event.objectId() + " не завершило ожидаемый шаг.");
        }
        return new TrialResult(completed, exercise.steps().size(), hints, details.size(), details);
    }
}
