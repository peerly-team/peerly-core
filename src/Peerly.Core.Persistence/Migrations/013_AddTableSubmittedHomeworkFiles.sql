-- +goose Up
-- +goose StatementBegin
create table submitted_homework_files
(
    submitted_homework_id bigint not null,
    file_id               bigint not null,
    anonymized_file_id    bigint,

    primary key (submitted_homework_id, file_id)
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table submitted_homework_files;
-- +goose StatementEnd
