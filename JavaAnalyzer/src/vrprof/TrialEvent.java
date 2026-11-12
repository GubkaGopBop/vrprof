package vrprof;

public record TrialEvent(int number, String kind, String stepCode, String objectId, boolean success) {
    public TrialEvent {
        if (number < 1 || !("action".equals(kind) || "hint".equals(kind)))
            throw new IllegalArgumentException("Некорректные номер или вид события.");
        if ("action".equals(kind) && (stepCode == null || stepCode.isBlank() || objectId == null || objectId.isBlank()))
            throw new IllegalArgumentException("Для действия нужны код и объект.");
    }
}
