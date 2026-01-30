-- +goose Up
-- +goose StatementBegin
create table student_homework_marks
(
    id                  bigserial primary key not null,
    student_id          bigint                not null,
    student_homework_id bigint                not null,
    mark                integer               not null,
    creation_time       timestamptz           not null,
    update_time         timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table student_homework_marks;
-- +goose StatementEnd
