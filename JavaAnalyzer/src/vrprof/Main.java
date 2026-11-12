package vrprof;

import com.google.gson.GsonBuilder;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;

public final class Main {
    public static void main(String[] args) {
        if (args.length < 1 || args.length > 2) {
            System.err.println("Использование: java -jar build/vrprof-analyzer.jar журнал.json [каталог-результата]");
            System.exit(2);
        }
        try {
            var analyzer = new TrialAnalyzer(new UnityJsonAttemptReader(), new AttemptEvaluator(),
                new TextSummaryFormatter(), Exercise.ramReplacement());
            var analysis = analyzer.analyze(Path.of(args[0]));
            String json = new GsonBuilder().setPrettyPrinting().disableHtmlEscaping().create().toJson(analysis);
            if (args.length == 2) {
                Path output = Path.of(args[1]);
                Files.createDirectories(output);
                Files.writeString(output.resolve("analysis.json"), json + "\n", StandardCharsets.UTF_8);
                Files.writeString(output.resolve("summary.txt"), analysis.summary() + "\n", StandardCharsets.UTF_8);
            }
            System.out.println(json);
        } catch (IOException | IllegalArgumentException ex) {
            System.err.println("Ошибка входных данных: " + ex.getMessage());
            System.exit(1);
        }
    }
}
