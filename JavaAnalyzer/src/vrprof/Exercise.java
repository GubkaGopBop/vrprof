package vrprof;

import java.util.List;

public record Exercise(String id, String title, List<TrialStep> steps) {
    public Exercise {
        if (id == null || id.isBlank() || title == null || title.isBlank() || steps == null || steps.isEmpty())
            throw new IllegalArgumentException("Задание должно иметь код, название и шаги.");
        steps = List.copyOf(steps);
    }

    public static Exercise ramReplacement() {
        return new Exercise("ram-replacement-v1", "Замена модуля оперативной памяти", List.of(
            new TrialStep("READ_REPORT", "diagnostic_report"), new TrialStep("POWER_OFF", "pc"),
            new TrialStep("UNPLUG", "power_cable"), new TrialStep("OPEN_CASE", "case"),
            new TrialStep("SELECT_FAULTY", "ram_slot_b"), new TrialStep("REMOVE_RAM", "ram_slot_b"),
            new TrialStep("INSTALL_RAM", "ram_slot_b"), new TrialStep("CLOSE_CASE", "case"),
            new TrialStep("RUN_TEST", "memory_test")));
    }
}
