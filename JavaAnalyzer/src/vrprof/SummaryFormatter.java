package vrprof;

public interface SummaryFormatter {
    String format(Exercise exercise, TrialAttempt attempt, TrialResult result);
}
