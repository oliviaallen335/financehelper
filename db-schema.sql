CREATE TABLE lectures (
  id INT AUTO_INCREMENT PRIMARY KEY,
  title VARCHAR(255) NOT NULL,
  source_file VARCHAR(255) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE lecture_chunks (
  id BIGINT AUTO_INCREMENT PRIMARY KEY,
  lecture_id INT NOT NULL,
  chunk_index INT NOT NULL,
  content TEXT NOT NULL,
  embedding JSON NULL,
  FOREIGN KEY (lecture_id) REFERENCES lectures(id)
);

CREATE TABLE chat_sessions (
  id BIGINT AUTO_INCREMENT PRIMARY KEY,
  user_label VARCHAR(100) NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE chat_messages (
  id BIGINT AUTO_INCREMENT PRIMARY KEY,
  session_id BIGINT NOT NULL,
  role ENUM('user', 'assistant', 'system', 'tool') NOT NULL,
  content TEXT NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (session_id) REFERENCES chat_sessions(id)
);

CREATE TABLE quizzes (
  id BIGINT AUTO_INCREMENT PRIMARY KEY,
  session_id BIGINT NOT NULL,
  topic VARCHAR(255) NOT NULL,
  quiz_json JSON NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (session_id) REFERENCES chat_sessions(id)
);
