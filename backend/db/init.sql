CREATE TABLE IF NOT EXISTS mensajes (
  id BIGSERIAL PRIMARY KEY,
  nombre VARCHAR(120) NOT NULL,
  email VARCHAR(255) NOT NULL,
  asunto VARCHAR(200) NOT NULL,
  texto TEXT NOT NULL,
  ip_usuario INET NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  read_at TIMESTAMPTZ NULL,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  deleted_at TIMESTAMPTZ NULL
);

CREATE INDEX IF NOT EXISTS idx_mensajes_ip_created_at
  ON mensajes (ip_usuario, created_at DESC)
  WHERE deleted_at IS NULL;