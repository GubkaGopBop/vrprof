package vrprof;

import java.io.IOException;
import java.nio.file.Path;
import java.util.Objects;

public final class TrialAnalyzer {
    private final AttemptReader reader;
    private final AttemptEvaluator evaluator;
    private final SummaryFormatter formatter;
    private final Exercise exercise;

    public TrialAnalyzer(AttemptReader reader, AttemptEvaluator evaluator, SummaryFormatter formatter, Exercise exercise) {
        this.reader = Objects.requireNonNull(reader);
        this.evaluator = Objects.requireNonNull(evaluator);
        this.formatter = Objects.requireNonNull(formatter);
        this.exercise = Objects.requireNonNull(exercise);
    }
    public Analysis analyze(Path path) throws IOException {
        TrialAttempt attempt = reader.read(path);
        TrialResult result = evaluator.evaluate(exercise, attempt);
        return new Analysis(attempt.attemptId(), attempt.pupilCode(), result.completed(), result,
            formatter.format(exercise, attempt, result));
    }
    public record Analysis(String attemptId, String pupilCode, boolean completed, TrialResult result, String summary) { }
}
