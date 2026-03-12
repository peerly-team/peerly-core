-- +goose Up
-- +goose StatementBegin
create table homework_files
(
    homework_id bigint not null,
    file_id     bigint not null,
    teacher_id  bigint not null,

    primary key (homework_id, file_id)
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table homework_files;
-- +goose StatementEnd
