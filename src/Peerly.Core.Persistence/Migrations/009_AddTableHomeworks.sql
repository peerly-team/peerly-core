-- +goose Up
-- +goose StatementBegin
create table homeworks
(
    id              bigserial primary key not null,
    course_id       bigint                not null,
    teacher_id      bigint                not null,
    name            text                  not null,
    description     text,
    checklist       text,
    status          text                  not null,
    deadline        timestamptz,
    review_deadline timestamptz,
    creation_time   timestamptz           not null,
    update_time     timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table homeworks;
-- +goose StatementEnd
