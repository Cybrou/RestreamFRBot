CREATE TABLE version (
	current TEXT NOT NULL,
	CONSTRAINT pk_version PRIMARY KEY (current)
) STRICT;

CREATE TABLE restream_module (
	id INT NOT NULL PRIMARY KEY,
	name TEXT NOT NULL
) STRICT;

CREATE TABLE restream_notif (
	restream_module_id INT NOT NULL REFERENCES restream_module(id),
	guid TEXT NOT NULL,
	sent_date INTEGER NOT NULL,
	PRIMARY KEY (restream_module_id, guid)
) STRICT;

INSERT INTO version VALUES ('1.0.0');
INSERT INTO restream_module VALUES (1, 'TPR S1');