package vrprof;

import java.io.IOException;
import java.nio.file.Path;

public interface AttemptReader {
    TrialAttempt read(Path path) throws IOException;
}
