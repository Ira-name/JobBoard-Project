namespace JobBoard.Api.Models;

public enum ApplicationStatus
{
    Pending,    // Очікує на розгляд
    Reviewed,   // Переглянуто рекрутером
    Accepted,   // Прийнято (оффер або наступний етап)
    Rejected    // Відхилено
}