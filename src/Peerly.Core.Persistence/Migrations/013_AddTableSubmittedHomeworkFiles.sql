-- +goose Up
-- +goose StatementBegin
create table submitted_homework_files
(
    submitted_homework_id bigint not null,
    file_id               bigint not null,

    primary key (submitted_homework_id, file_id)
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table submitted_homework_files;
-- +goose StatementEnd
