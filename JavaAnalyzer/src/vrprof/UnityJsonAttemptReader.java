package vrprof;

import com.google.gson.*;
import com.google.gson.stream.JsonReader;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;

public final class UnityJsonAttemptReader implements AttemptReader {
    public TrialAttempt read(Path path) throws IOException {
        try (var source = Files.newBufferedReader(path, StandardCharsets.UTF_8)) {
            var jsonReader = new JsonReader(source);
            jsonReader.setStrictness(Strictness.STRICT);
            JsonObject root = JsonParser.parseReader(jsonReader).getAsJsonObject();
            if (jsonReader.peek() != com.google.gson.stream.JsonToken.END_DOCUMENT)
                throw new IllegalArgumentException("После JSON есть лишние данные.");
            var events = new ArrayList<TrialEvent>();
            JsonElement rawEvents = required(root, "Events");
            if (!rawEvents.isJsonArray()) throw new IllegalArgumentException("Events должен быть массивом.");
            for (JsonElement element : rawEvents.getAsJsonArray()) {
                JsonObject item = element.getAsJsonObject();
                String kind = text(item, "Kind");
                events.add(new TrialEvent(integer(item, "Number"), kind,
                    kind.equals("action") ? text(item, "StepCode") : "",
                    kind.equals("action") ? text(item, "ObjectId") : "",
                    kind.equals("action") && bool(item, "Success")));
            }
            // Summary из входа намеренно не читается: результат вычисляется по событиям.
            return new TrialAttempt(text(root, "AttemptId"), text(root, "ExerciseId"),
                text(root, "PupilCode"), text(root, "MentorCode"), events,
                integer(root, "Difficulty"), integer(root, "Interest"),
                text(root, "StudentFeedback"), text(root, "MentorFeedback"));
        } catch (JsonParseException | IllegalStateException | ArithmeticException ex) {
            throw new IllegalArgumentException("Некорректный JSON-журнал: " + ex.getMessage(), ex);
        }
    }

    private static JsonElement required(JsonObject object, String key) {
        JsonElement value = object.get(key);
        if (value == null || value.isJsonNull()) throw new IllegalArgumentException("Нет поля " + key);
        return value;
    }
    private static String text(JsonObject object, String key) {
        JsonElement value = required(object, key);
        if (!value.isJsonPrimitive() || !value.getAsJsonPrimitive().isString())
            throw new IllegalArgumentException(key + " должен быть строкой.");
        return value.getAsString();
    }
    private static int integer(JsonObject object, String key) {
        JsonElement value = required(object, key);
        if (!value.isJsonPrimitive() || !value.getAsJsonPrimitive().isNumber())
            throw new IllegalArgumentException(key + " должен быть целым числом.");
        return value.getAsBigDecimal().intValueExact();
    }
    private static boolean bool(JsonObject object, String key) {
        JsonElement value = required(object, key);
        if (!value.isJsonPrimitive() || !value.getAsJsonPrimitive().isBoolean())
            throw new IllegalArgumentException(key + " должен быть true или false.");
        return value.getAsBoolean();
    }
}
