package vrprof;

import java.util.List;

public record TrialResult(int completedSteps, int totalSteps, int hints, int errors, List<String> errorDetails) {
    public TrialResult {
        if (totalSteps < 1 || completedSteps < 0 || completedSteps > totalSteps || hints < 0
                || errors < 0 || errorDetails == null || errors != errorDetails.size())
            throw new IllegalArgumentException("Несогласованный результат проверки.");
        errorDetails = List.copyOf(errorDetails);
    }
    public boolean completed() { return completedSteps == totalSteps; }
}
