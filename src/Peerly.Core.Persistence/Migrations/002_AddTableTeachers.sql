-- +goose Up
-- +goose StatementBegin
create table teachers
(
    id            bigint primary key not null,
    email         text               not null,
    name          text,
    creation_time timestamptz        not null,
    update_time   timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table teachers;
-- +goose StatementEnd
