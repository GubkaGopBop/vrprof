package vrprof;

public record TrialStep(String code, String objectId) {
    public TrialStep {
        if (code == null || code.isBlank() || objectId == null || objectId.isBlank())
            throw new IllegalArgumentException("Код шага и объект обязательны.");
    }
}
