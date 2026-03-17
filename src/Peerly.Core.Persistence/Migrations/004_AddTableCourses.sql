-- +goose Up
-- +goose StatementBegin
create table courses
(
    id            bigserial primary key not null,
    name          text                  not null,
    status        text                  not null,
    description   text,
    creation_time timestamptz           not null,
    update_time   timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table courses;
-- +goose StatementEnd
